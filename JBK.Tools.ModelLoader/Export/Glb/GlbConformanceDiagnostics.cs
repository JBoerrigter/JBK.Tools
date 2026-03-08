using System.Text;
using System.Text.Json;

namespace JBK.Tools.ModelLoader.Export.Glb;

internal static class GlbConformanceDiagnostics
{
    private const uint GlbMagic = 0x46546C67; // "glTF"
    private const uint GlbVersion2 = 2;
    private const uint JsonChunkType = 0x4E4F534A; // "JSON"
    private const uint BinChunkType = 0x004E4942;  // "BIN\0"

    private const int ArrayBufferTarget = 34962;
    private const int ElementArrayBufferTarget = 34963;

    private const int ComponentByte = 5120;
    private const int ComponentUByte = 5121;
    private const int ComponentShort = 5122;
    private const int ComponentUShort = 5123;
    private const int ComponentUInt = 5125;
    private const int ComponentFloat = 5126;

    private sealed record BufferDef(int ByteLength);
    private sealed record BufferViewDef(int Buffer, int ByteOffset, int ByteLength, int? ByteStride, int? Target);
    private sealed record AccessorDef(
        int? BufferView,
        int ByteOffset,
        int Count,
        string Type,
        int ComponentType,
        bool Normalized,
        bool HasMin,
        bool HasMax);

    private sealed class Report
    {
        public List<string> Diagnostics { get; } = new();
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();
    }

    public static void ValidateAndReport(string glbPath, bool exportDiagnostics)
    {
        var report = Analyze(glbPath);

        if (exportDiagnostics)
        {
            foreach (var line in report.Diagnostics)
            {
                Console.WriteLine(line);
            }
        }

        foreach (var warning in report.Warnings)
        {
            Console.Error.WriteLine($"[GLB.STRICT][WARN] {warning}");
        }

        if (report.Errors.Count == 0)
        {
            return;
        }

        foreach (var error in report.Errors)
        {
            Console.Error.WriteLine($"[GLB.STRICT][ERROR] {error}");
        }

        throw new InvalidDataException(
            $"Strict GLB conformance checks failed for '{glbPath}' with {report.Errors.Count} error(s).");
    }

    private static Report Analyze(string glbPath)
    {
        var report = new Report();
        var bytes = File.ReadAllBytes(glbPath);

        if (bytes.Length < 20)
        {
            report.Errors.Add($"GLB is too small to contain header + JSON chunk: {bytes.Length} bytes.");
            return report;
        }

        using var stream = new MemoryStream(bytes, writable: false);
        using var reader = new BinaryReader(stream);

        uint magic = reader.ReadUInt32();
        uint version = reader.ReadUInt32();
        uint declaredLength = reader.ReadUInt32();

        report.Diagnostics.Add($"[GLB.CHUNK] header.magic=0x{magic:X8} header.version={version} header.length={declaredLength}");

        if (magic != GlbMagic)
        {
            report.Errors.Add("header.magic is not 'glTF' (0x46546C67).");
        }

        if (version != GlbVersion2)
        {
            report.Errors.Add($"header.version is {version}, expected 2.");
        }

        if (declaredLength != bytes.Length)
        {
            report.Errors.Add($"header.length={declaredLength} does not match actual file length={bytes.Length}.");
        }

        long jsonChunkStart = stream.Position;
        if (stream.Length - stream.Position < 8)
        {
            report.Errors.Add("Missing JSON chunk header.");
            return report;
        }

        int jsonLength = reader.ReadInt32();
        uint jsonType = reader.ReadUInt32();
        report.Diagnostics.Add($"[GLB.CHUNK] type=JSON offset={jsonChunkStart} length={jsonLength}");

        if (jsonType != JsonChunkType)
        {
            report.Errors.Add($"First chunk type is 0x{jsonType:X8}, expected JSON (0x4E4F534A).");
        }

        if (jsonLength < 0)
        {
            report.Errors.Add($"JSON chunk length is negative: {jsonLength}.");
            return report;
        }

        if (jsonLength % 4 != 0)
        {
            report.Errors.Add($"JSON chunk length is not 4-byte aligned: {jsonLength}.");
        }

        if (stream.Length - stream.Position < jsonLength)
        {
            report.Errors.Add("JSON chunk extends beyond file length.");
            return report;
        }

        byte[] jsonChunk = reader.ReadBytes(jsonLength);
        if (!HasOnlyJsonPadding(jsonChunk))
        {
            report.Warnings.Add("JSON chunk padding should use spaces (0x20).");
        }

        int? binChunkLength = null;
        byte[] binChunk = Array.Empty<byte>();

        if (stream.Position < stream.Length)
        {
            long binChunkStart = stream.Position;
            if (stream.Length - stream.Position < 8)
            {
                report.Errors.Add("Missing BIN chunk header bytes.");
                return report;
            }

            int binLength = reader.ReadInt32();
            uint binType = reader.ReadUInt32();
            report.Diagnostics.Add($"[GLB.CHUNK] type=BIN offset={binChunkStart} length={binLength}");

            if (binType != BinChunkType)
            {
                report.Errors.Add($"Second chunk type is 0x{binType:X8}, expected BIN\\0 (0x004E4942).");
            }

            if (binLength < 0)
            {
                report.Errors.Add($"BIN chunk length is negative: {binLength}.");
                return report;
            }

            if (binLength % 4 != 0)
            {
                report.Errors.Add($"BIN chunk length is not 4-byte aligned: {binLength}.");
            }

            if (stream.Length - stream.Position < binLength)
            {
                report.Errors.Add("BIN chunk extends beyond file length.");
                return report;
            }

            binChunk = reader.ReadBytes(binLength);
            binChunkLength = binLength;
        }

        if (stream.Position != stream.Length)
        {
            report.Errors.Add("Extra bytes found after final GLB chunk.");
        }

        string jsonText = DecodeJsonChunk(jsonChunk);
        if (string.IsNullOrWhiteSpace(jsonText))
        {
            report.Errors.Add("JSON chunk is empty.");
            return report;
        }

        JsonDocument jsonDoc;
        try
        {
            jsonDoc = JsonDocument.Parse(jsonText);
        }
        catch (JsonException ex)
        {
            report.Errors.Add($"Failed to parse JSON chunk: {ex.Message}");
            return report;
        }

        using (jsonDoc)
        {
            AnalyzeSchema(jsonDoc.RootElement, binChunkLength, binChunk, report);
        }

        return report;
    }

    private static void AnalyzeSchema(JsonElement root, int? binChunkLength, byte[] binChunk, Report report)
    {
        var buffers = ParseBuffers(root, report);
        var bufferViews = ParseBufferViews(root, report);
        var accessors = ParseAccessors(root, report);

        int nodesCount = GetArrayLength(root, "nodes");
        int meshesCount = GetArrayLength(root, "meshes");
        int materialsCount = GetArrayLength(root, "materials");

        if (buffers.Count == 0)
        {
            if (binChunkLength.HasValue || bufferViews.Count > 0 || accessors.Count > 0)
            {
                report.Errors.Add("Missing buffers array.");
            }
        }
        else if (binChunkLength.HasValue && buffers[0].ByteLength != binChunkLength.Value)
        {
            report.Errors.Add($"/buffers/0/byteLength={buffers[0].ByteLength} does not match BIN chunk length={binChunkLength.Value}.");
        }

        for (int i = 0; i < bufferViews.Count; i++)
        {
            var view = bufferViews[i];
            string pointer = $"/bufferViews/{i}";

            if (view.Buffer < 0 || view.Buffer >= buffers.Count)
            {
                report.Errors.Add($"{pointer}/buffer references invalid buffer index {view.Buffer}.");
                continue;
            }

            if (view.ByteOffset < 0 || view.ByteLength < 0)
            {
                report.Errors.Add($"{pointer} has negative byteOffset or byteLength.");
                continue;
            }

            long viewEnd = (long)view.ByteOffset + view.ByteLength;
            if (viewEnd > buffers[view.Buffer].ByteLength)
            {
                report.Errors.Add($"{pointer} range [{view.ByteOffset},{viewEnd}) exceeds /buffers/{view.Buffer}/byteLength={buffers[view.Buffer].ByteLength}.");
            }

            if (view.Target is ArrayBufferTarget or ElementArrayBufferTarget && view.ByteOffset % 4 != 0)
            {
                report.Warnings.Add($"{pointer}/byteOffset={view.ByteOffset} is not 4-byte aligned for target={view.Target}.");
            }
        }

        for (int i = 0; i < accessors.Count; i++)
        {
            var accessor = accessors[i];
            string pointer = $"/accessors/{i}";
            int componentSize = GetComponentSize(accessor.ComponentType);
            int elementComponents = GetTypeComponentCount(accessor.Type);

            if (componentSize <= 0)
            {
                report.Errors.Add($"{pointer}/componentType={accessor.ComponentType} is invalid.");
                continue;
            }

            if (elementComponents <= 0)
            {
                report.Errors.Add($"{pointer}/type='{accessor.Type}' is invalid.");
                continue;
            }

            if (accessor.ByteOffset < 0)
            {
                report.Errors.Add($"{pointer}/byteOffset is negative.");
            }
            else if (accessor.ByteOffset % componentSize != 0)
            {
                report.Errors.Add($"{pointer}/byteOffset={accessor.ByteOffset} is not aligned to component size {componentSize}.");
            }

            if (accessor.Count < 0)
            {
                report.Errors.Add($"{pointer}/count is negative.");
                continue;
            }

            if (!accessor.BufferView.HasValue)
            {
                continue;
            }

            int viewIndex = accessor.BufferView.Value;
            if (viewIndex < 0 || viewIndex >= bufferViews.Count)
            {
                report.Errors.Add($"{pointer}/bufferView references invalid index {viewIndex}.");
                continue;
            }

            var view = bufferViews[viewIndex];
            int elementSize = componentSize * elementComponents;
            int stride = view.ByteStride ?? elementSize;

            if (stride < elementSize)
            {
                report.Errors.Add($"{pointer} elementSize={elementSize} exceeds /bufferViews/{viewIndex}/byteStride={stride}.");
                continue;
            }

            long usedLength = accessor.Count == 0 ? 0 : (long)stride * (accessor.Count - 1) + elementSize;
            long accessorEnd = (long)accessor.ByteOffset + usedLength;
            if (accessorEnd > view.ByteLength)
            {
                report.Errors.Add($"{pointer} range exceeds /bufferViews/{viewIndex} length. accessorEnd={accessorEnd}, viewLength={view.ByteLength}.");
            }
        }

        AnalyzeMeshes(root, accessors, meshesCount, materialsCount, report);
        AnalyzeSkinsAndNodes(root, accessors, nodesCount, meshesCount, report);
        AnalyzeAnimations(root, accessors, bufferViews, binChunk, nodesCount, report);
        AnalyzeNodeTransforms(root, report);
    }

    private static void AnalyzeMeshes(
        JsonElement root,
        IReadOnlyList<AccessorDef> accessors,
        int meshesCount,
        int materialsCount,
        Report report)
    {
        if (!TryGetArray(root, "meshes", out var meshesArray))
        {
            return;
        }

        for (int meshIndex = 0; meshIndex < meshesArray.GetArrayLength(); meshIndex++)
        {
            var mesh = meshesArray[meshIndex];
            if (!TryGetArray(mesh, "primitives", out var primitives))
            {
                report.Errors.Add($"/meshes/{meshIndex}/primitives is missing.");
                continue;
            }

            for (int primIndex = 0; primIndex < primitives.GetArrayLength(); primIndex++)
            {
                var primitive = primitives[primIndex];
                string primPointer = $"/meshes/{meshIndex}/primitives/{primIndex}";

                if (!TryGetObject(primitive, "attributes", out var attrs))
                {
                    report.Errors.Add($"{primPointer}/attributes is missing.");
                    continue;
                }

                var attrSummary = new List<string>();
                foreach (var attr in attrs.EnumerateObject())
                {
                    if (attr.Value.ValueKind == JsonValueKind.Number && attr.Value.TryGetInt32(out int acc))
                    {
                        attrSummary.Add($"{attr.Name}:{acc}");
                    }
                }

                report.Diagnostics.Add($"[GLB.ATTR] mesh={meshIndex} primitive={primIndex} attrs=[{string.Join(", ", attrSummary)}]");

                if (!TryGetInt(attrs, "POSITION", out int positionAccessor))
                {
                    report.Errors.Add($"{primPointer}/attributes/POSITION is required.");
                }
                else
                {
                    ValidateAttributeAccessor(accessors, positionAccessor, "VEC3", ComponentFloat, $"{primPointer}/attributes/POSITION", report);
                    if (positionAccessor >= 0 && positionAccessor < accessors.Count)
                    {
                        var acc = accessors[positionAccessor];
                        if (!acc.HasMin || !acc.HasMax)
                        {
                            report.Warnings.Add($"{primPointer}/attributes/POSITION accessor should provide min/max.");
                        }
                    }
                }

                if (TryGetInt(attrs, "NORMAL", out int normalAccessor))
                {
                    ValidateAttributeAccessor(accessors, normalAccessor, "VEC3", ComponentFloat, $"{primPointer}/attributes/NORMAL", report);
                }

                if (TryGetInt(attrs, "TANGENT", out int tangentAccessor))
                {
                    ValidateAttributeAccessor(accessors, tangentAccessor, "VEC4", ComponentFloat, $"{primPointer}/attributes/TANGENT", report);
                }

                foreach (var attr in attrs.EnumerateObject())
                {
                    if (!attr.Name.StartsWith("TEXCOORD_", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!attr.Value.TryGetInt32(out int uvAccessor))
                    {
                        report.Errors.Add($"{primPointer}/attributes/{attr.Name} is not an accessor index.");
                        continue;
                    }

                    if (!TryGetAccessor(accessors, uvAccessor, out var accessor, out string uvPointer))
                    {
                        report.Errors.Add($"{primPointer}/attributes/{attr.Name} references invalid accessor {uvAccessor}.");
                        continue;
                    }

                    if (!string.Equals(accessor.Type, "VEC2", StringComparison.Ordinal))
                    {
                        report.Errors.Add($"{uvPointer}/type must be VEC2 for {attr.Name}.");
                    }

                    bool validComponent =
                        accessor.ComponentType == ComponentFloat ||
                        accessor.ComponentType == ComponentUShort ||
                        accessor.ComponentType == ComponentUByte;
                    if (!validComponent)
                    {
                        report.Errors.Add($"{uvPointer}/componentType={accessor.ComponentType} is invalid for {attr.Name}.");
                    }

                    if (accessor.ComponentType is ComponentUShort or ComponentUByte && !accessor.Normalized)
                    {
                        report.Errors.Add($"{uvPointer} must set normalized=true when using integer TEXCOORD componentType.");
                    }
                }

                if (TryGetInt(attrs, "JOINTS_0", out int jointsAccessor))
                {
                    if (!TryGetAccessor(accessors, jointsAccessor, out var accessor, out string jointsPointer))
                    {
                        report.Errors.Add($"{primPointer}/attributes/JOINTS_0 references invalid accessor {jointsAccessor}.");
                    }
                    else
                    {
                        if (!string.Equals(accessor.Type, "VEC4", StringComparison.Ordinal))
                        {
                            report.Errors.Add($"{jointsPointer}/type must be VEC4.");
                        }

                        if (accessor.ComponentType != ComponentUByte && accessor.ComponentType != ComponentUShort)
                        {
                            report.Errors.Add($"{jointsPointer}/componentType must be UNSIGNED_BYTE or UNSIGNED_SHORT.");
                        }
                    }
                }

                if (TryGetInt(attrs, "WEIGHTS_0", out int weightsAccessor))
                {
                    ValidateAttributeAccessor(accessors, weightsAccessor, "VEC4", ComponentFloat, $"{primPointer}/attributes/WEIGHTS_0", report);
                }

                if (TryGetInt(primitive, "indices", out int indexAccessor))
                {
                    if (!TryGetAccessor(accessors, indexAccessor, out var accessor, out string indexPointer))
                    {
                        report.Errors.Add($"{primPointer}/indices references invalid accessor {indexAccessor}.");
                    }
                    else
                    {
                        if (!string.Equals(accessor.Type, "SCALAR", StringComparison.Ordinal))
                        {
                            report.Errors.Add($"{indexPointer}/type must be SCALAR for indices.");
                        }

                        if (accessor.ComponentType != ComponentUShort && accessor.ComponentType != ComponentUInt)
                        {
                            report.Errors.Add($"{indexPointer}/componentType must be UNSIGNED_SHORT or UNSIGNED_INT for indices.");
                        }
                    }
                }

                if (TryGetInt(primitive, "material", out int materialIndex))
                {
                    if (materialIndex < 0 || materialIndex >= materialsCount)
                    {
                        report.Errors.Add($"{primPointer}/material references invalid material index {materialIndex}.");
                    }
                }
            }
        }
    }

    private static void AnalyzeSkinsAndNodes(
        JsonElement root,
        IReadOnlyList<AccessorDef> accessors,
        int nodesCount,
        int meshesCount,
        Report report)
    {
        var skinnedMeshes = new HashSet<int>();

        if (TryGetArray(root, "nodes", out var nodesArray))
        {
            for (int nodeIndex = 0; nodeIndex < nodesArray.GetArrayLength(); nodeIndex++)
            {
                var node = nodesArray[nodeIndex];
                string nodePointer = $"/nodes/{nodeIndex}";

                if (TryGetInt(node, "mesh", out int meshIndex))
                {
                    if (meshIndex < 0 || meshIndex >= meshesCount)
                    {
                        report.Errors.Add($"{nodePointer}/mesh references invalid mesh index {meshIndex}.");
                    }
                }

                if (TryGetInt(node, "skin", out int skinIndex))
                {
                    if (TryGetInt(node, "mesh", out int meshIndexForSkin) && meshIndexForSkin >= 0)
                    {
                        skinnedMeshes.Add(meshIndexForSkin);
                    }

                    if (!TryGetArray(root, "skins", out var skinsArray) ||
                        skinIndex < 0 ||
                        skinIndex >= skinsArray.GetArrayLength())
                    {
                        report.Errors.Add($"{nodePointer}/skin references invalid skin index {skinIndex}.");
                    }
                }
            }
        }

        if (TryGetArray(root, "meshes", out var meshesArray))
        {
            foreach (int meshIndex in skinnedMeshes)
            {
                if (meshIndex < 0 || meshIndex >= meshesArray.GetArrayLength())
                {
                    continue;
                }

                var mesh = meshesArray[meshIndex];
                if (!TryGetArray(mesh, "primitives", out var primitives))
                {
                    continue;
                }

                for (int primIndex = 0; primIndex < primitives.GetArrayLength(); primIndex++)
                {
                    var primitive = primitives[primIndex];
                    string primPointer = $"/meshes/{meshIndex}/primitives/{primIndex}";

                    if (!TryGetObject(primitive, "attributes", out var attrs))
                    {
                        report.Errors.Add($"{primPointer}/attributes is missing for skinned primitive.");
                        continue;
                    }

                    if (!attrs.TryGetProperty("JOINTS_0", out _) || !attrs.TryGetProperty("WEIGHTS_0", out _))
                    {
                        report.Errors.Add($"{primPointer} is skinned but JOINTS_0/WEIGHTS_0 are missing.");
                    }
                }
            }
        }

        if (!TryGetArray(root, "skins", out var skins))
        {
            return;
        }

        for (int skinIndex = 0; skinIndex < skins.GetArrayLength(); skinIndex++)
        {
            var skin = skins[skinIndex];
            string skinPointer = $"/skins/{skinIndex}";

            if (!TryGetArray(skin, "joints", out var joints))
            {
                report.Errors.Add($"{skinPointer}/joints is missing.");
                continue;
            }

            report.Diagnostics.Add($"[GLB.SKIN] skin={skinIndex} joints={joints.GetArrayLength()}");

            for (int i = 0; i < joints.GetArrayLength(); i++)
            {
                int jointNode = joints[i].GetInt32();
                if (jointNode < 0 || jointNode >= nodesCount)
                {
                    report.Errors.Add($"{skinPointer}/joints/{i} references invalid node index {jointNode}.");
                }
            }

            if (TryGetInt(skin, "skeleton", out int skeletonNode) &&
                (skeletonNode < 0 || skeletonNode >= nodesCount))
            {
                report.Errors.Add($"{skinPointer}/skeleton references invalid node index {skeletonNode}.");
            }

            if (!TryGetInt(skin, "inverseBindMatrices", out int ibmAccessor))
            {
                continue;
            }

            if (!TryGetAccessor(accessors, ibmAccessor, out var accessor, out string accessorPointer))
            {
                report.Errors.Add($"{skinPointer}/inverseBindMatrices references invalid accessor {ibmAccessor}.");
                continue;
            }

            if (!string.Equals(accessor.Type, "MAT4", StringComparison.Ordinal))
            {
                report.Errors.Add($"{accessorPointer}/type must be MAT4 for inverseBindMatrices.");
            }

            if (accessor.ComponentType != ComponentFloat)
            {
                report.Errors.Add($"{accessorPointer}/componentType must be FLOAT for inverseBindMatrices.");
            }

            if (accessor.Count != joints.GetArrayLength())
            {
                report.Errors.Add($"{accessorPointer}/count={accessor.Count} does not match {skinPointer}/joints length={joints.GetArrayLength()}.");
            }
        }
    }

    private static void AnalyzeAnimations(
        JsonElement root,
        IReadOnlyList<AccessorDef> accessors,
        IReadOnlyList<BufferViewDef> bufferViews,
        byte[] binChunk,
        int nodesCount,
        Report report)
    {
        if (!TryGetArray(root, "animations", out var animations))
        {
            return;
        }

        for (int animationIndex = 0; animationIndex < animations.GetArrayLength(); animationIndex++)
        {
            var animation = animations[animationIndex];
            string animationPointer = $"/animations/{animationIndex}";

            if (!TryGetArray(animation, "samplers", out var samplers))
            {
                report.Errors.Add($"{animationPointer}/samplers is missing.");
                continue;
            }

            if (!TryGetArray(animation, "channels", out var channels))
            {
                report.Errors.Add($"{animationPointer}/channels is missing.");
                continue;
            }

            report.Diagnostics.Add($"[GLB.ANIM] animation={animationIndex} samplers={samplers.GetArrayLength()} channels={channels.GetArrayLength()}");

            for (int samplerIndex = 0; samplerIndex < samplers.GetArrayLength(); samplerIndex++)
            {
                var sampler = samplers[samplerIndex];
                string samplerPointer = $"{animationPointer}/samplers/{samplerIndex}";

                if (!TryGetInt(sampler, "input", out int inputAccessor) ||
                    !TryGetAccessor(accessors, inputAccessor, out var input, out string inputPointer))
                {
                    report.Errors.Add($"{samplerPointer}/input references invalid accessor.");
                    continue;
                }

                if (!TryGetInt(sampler, "output", out int outputAccessor) ||
                    !TryGetAccessor(accessors, outputAccessor, out var output, out string outputPointer))
                {
                    report.Errors.Add($"{samplerPointer}/output references invalid accessor.");
                    continue;
                }

                if (!string.Equals(input.Type, "SCALAR", StringComparison.Ordinal) || input.ComponentType != ComponentFloat)
                {
                    report.Errors.Add($"{inputPointer} must be SCALAR FLOAT for animation input.");
                }

                if (!TryReadFloatAccessor(accessors, bufferViews, binChunk, inputAccessor, out var timeData, out string inputReadError))
                {
                    report.Errors.Add($"{inputPointer}: {inputReadError}");
                }
                else
                {
                    var times = timeData.Select(static row => row[0]).ToArray();
                    if (!IsStrictlyIncreasing(times))
                    {
                        report.Errors.Add($"{inputPointer} keyframe times must be strictly increasing.");
                    }
                }

                string interpolation = TryGetString(sampler, "interpolation") ?? "LINEAR";
                if (!string.Equals(interpolation, "LINEAR", StringComparison.Ordinal) &&
                    !string.Equals(interpolation, "STEP", StringComparison.Ordinal) &&
                    !string.Equals(interpolation, "CUBICSPLINE", StringComparison.Ordinal))
                {
                    report.Errors.Add($"{samplerPointer}/interpolation '{interpolation}' is invalid.");
                }

                if (string.Equals(interpolation, "CUBICSPLINE", StringComparison.Ordinal) &&
                    output.Count != input.Count * 3)
                {
                    report.Errors.Add($"{outputPointer}/count={output.Count} must equal 3 * input count ({input.Count}) for CUBICSPLINE.");
                }
            }

            for (int channelIndex = 0; channelIndex < channels.GetArrayLength(); channelIndex++)
            {
                var channel = channels[channelIndex];
                string channelPointer = $"{animationPointer}/channels/{channelIndex}";

                if (!TryGetInt(channel, "sampler", out int samplerIndex) ||
                    samplerIndex < 0 ||
                    samplerIndex >= samplers.GetArrayLength())
                {
                    report.Errors.Add($"{channelPointer}/sampler references invalid sampler index.");
                    continue;
                }

                if (!TryGetObject(channel, "target", out var target))
                {
                    report.Errors.Add($"{channelPointer}/target is missing.");
                    continue;
                }

                if (!TryGetInt(target, "node", out int targetNode) || targetNode < 0 || targetNode >= nodesCount)
                {
                    report.Errors.Add($"{channelPointer}/target/node references invalid node index.");
                }

                string path = TryGetString(target, "path") ?? string.Empty;
                if (!string.Equals(path, "translation", StringComparison.Ordinal) &&
                    !string.Equals(path, "rotation", StringComparison.Ordinal) &&
                    !string.Equals(path, "scale", StringComparison.Ordinal) &&
                    !string.Equals(path, "weights", StringComparison.Ordinal))
                {
                    report.Errors.Add($"{channelPointer}/target/path '{path}' is invalid.");
                    continue;
                }

                var sampler = samplers[samplerIndex];
                if (!TryGetInt(sampler, "output", out int outputAccessor) ||
                    !TryGetAccessor(accessors, outputAccessor, out var output, out string outputPointer))
                {
                    report.Errors.Add($"{channelPointer} references invalid output accessor.");
                    continue;
                }

                if (string.Equals(path, "translation", StringComparison.Ordinal))
                {
                    ValidateAnimationOutputAccessor(output, outputPointer, "VEC3", report);
                }
                else if (string.Equals(path, "rotation", StringComparison.Ordinal))
                {
                    ValidateAnimationOutputAccessor(output, outputPointer, "VEC4", report);

                    if (TryReadFloatAccessor(accessors, bufferViews, binChunk, outputAccessor, out var quatData, out string readError))
                    {
                        for (int i = 0; i < quatData.Length; i++)
                        {
                            float x = quatData[i][0];
                            float y = quatData[i][1];
                            float z = quatData[i][2];
                            float w = quatData[i][3];
                            float len = MathF.Sqrt((x * x) + (y * y) + (z * z) + (w * w));
                            if (!float.IsFinite(len) || len < 1e-6f)
                            {
                                report.Errors.Add($"{outputPointer} quaternion[{i}] is invalid (length={len}).");
                                continue;
                            }

                            if (MathF.Abs(len - 1f) > 1e-3f)
                            {
                                report.Warnings.Add($"{outputPointer} quaternion[{i}] length={len} is not normalized.");
                            }
                        }
                    }
                    else
                    {
                        report.Errors.Add($"{outputPointer}: {readError}");
                    }
                }
                else if (string.Equals(path, "scale", StringComparison.Ordinal))
                {
                    ValidateAnimationOutputAccessor(output, outputPointer, "VEC3", report);
                }

                if (TryReadFloatAccessor(accessors, bufferViews, binChunk, outputAccessor, out var data, out string outputReadError))
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        for (int c = 0; c < data[i].Length; c++)
                        {
                            if (!float.IsFinite(data[i][c]))
                            {
                                report.Errors.Add($"{outputPointer} contains non-finite value at [{i},{c}].");
                            }
                        }
                    }
                }
                else
                {
                    report.Errors.Add($"{outputPointer}: {outputReadError}");
                }
            }
        }
    }

    private static void AnalyzeNodeTransforms(JsonElement root, Report report)
    {
        if (!TryGetArray(root, "nodes", out var nodes))
        {
            return;
        }

        for (int nodeIndex = 0; nodeIndex < nodes.GetArrayLength(); nodeIndex++)
        {
            var node = nodes[nodeIndex];
            string nodePointer = $"/nodes/{nodeIndex}";

            bool hasMatrix = node.TryGetProperty("matrix", out var matrix);
            bool hasTranslation = node.TryGetProperty("translation", out var translation);
            bool hasRotation = node.TryGetProperty("rotation", out var rotation);
            bool hasScale = node.TryGetProperty("scale", out var scale);

            if (hasMatrix && (hasTranslation || hasRotation || hasScale))
            {
                report.Errors.Add($"{nodePointer} must define either matrix or TRS, not both.");
            }

            if (hasMatrix && !IsArrayOfFiniteNumbers(matrix, 16))
            {
                report.Errors.Add($"{nodePointer}/matrix must be a 16-number finite array.");
            }

            if (hasTranslation && !IsArrayOfFiniteNumbers(translation, 3))
            {
                report.Errors.Add($"{nodePointer}/translation must be a 3-number finite array.");
            }

            if (hasRotation && !IsArrayOfFiniteNumbers(rotation, 4))
            {
                report.Errors.Add($"{nodePointer}/rotation must be a 4-number finite array.");
            }

            if (hasScale && !IsArrayOfFiniteNumbers(scale, 3))
            {
                report.Errors.Add($"{nodePointer}/scale must be a 3-number finite array.");
            }
        }
    }

    private static bool TryReadFloatAccessor(
        IReadOnlyList<AccessorDef> accessors,
        IReadOnlyList<BufferViewDef> bufferViews,
        byte[] binChunk,
        int accessorIndex,
        out float[][] data,
        out string error)
    {
        data = Array.Empty<float[]>();
        error = string.Empty;

        if (!TryGetAccessor(accessors, accessorIndex, out var accessor, out string accessorPointer))
        {
            error = $"Invalid accessor index {accessorIndex}.";
            return false;
        }

        if (accessor.ComponentType != ComponentFloat)
        {
            error = "Accessor componentType is not FLOAT.";
            return false;
        }

        if (!accessor.BufferView.HasValue)
        {
            error = "Accessor has no bufferView.";
            return false;
        }

        int viewIndex = accessor.BufferView.Value;
        if (viewIndex < 0 || viewIndex >= bufferViews.Count)
        {
            error = $"Accessor references invalid bufferView {viewIndex}.";
            return false;
        }

        var view = bufferViews[viewIndex];
        int componentCount = GetTypeComponentCount(accessor.Type);
        if (componentCount <= 0)
        {
            error = $"Unsupported accessor type '{accessor.Type}'.";
            return false;
        }

        int elementSize = componentCount * sizeof(float);
        int stride = view.ByteStride ?? elementSize;
        if (stride < elementSize)
        {
            error = "bufferView.byteStride is smaller than element size.";
            return false;
        }

        if (binChunk.Length == 0)
        {
            error = "BIN chunk is empty.";
            return false;
        }

        data = new float[accessor.Count][];
        int baseOffset = view.ByteOffset + accessor.ByteOffset;
        for (int element = 0; element < accessor.Count; element++)
        {
            int elementOffset = baseOffset + (element * stride);
            int elementEnd = elementOffset + elementSize;
            if (elementOffset < 0 || elementEnd > binChunk.Length)
            {
                error = $"{accessorPointer} reads beyond BIN chunk bounds.";
                return false;
            }

            var values = new float[componentCount];
            for (int c = 0; c < componentCount; c++)
            {
                int componentOffset = elementOffset + (c * sizeof(float));
                values[c] = BitConverter.ToSingle(binChunk, componentOffset);
            }

            data[element] = values;
        }

        return true;
    }

    private static void ValidateAnimationOutputAccessor(AccessorDef accessor, string pointer, string expectedType, Report report)
    {
        if (!string.Equals(accessor.Type, expectedType, StringComparison.Ordinal))
        {
            report.Errors.Add($"{pointer}/type must be {expectedType} for animation output.");
        }

        if (accessor.ComponentType != ComponentFloat)
        {
            report.Errors.Add($"{pointer}/componentType must be FLOAT for animation output.");
        }
    }

    private static void ValidateAttributeAccessor(
        IReadOnlyList<AccessorDef> accessors,
        int accessorIndex,
        string expectedType,
        int expectedComponentType,
        string pointer,
        Report report)
    {
        if (!TryGetAccessor(accessors, accessorIndex, out var accessor, out var accessorPointer))
        {
            report.Errors.Add($"{pointer} references invalid accessor index {accessorIndex}.");
            return;
        }

        if (!string.Equals(accessor.Type, expectedType, StringComparison.Ordinal))
        {
            report.Errors.Add($"{accessorPointer}/type must be {expectedType} for {pointer}.");
        }

        if (accessor.ComponentType != expectedComponentType)
        {
            report.Errors.Add($"{accessorPointer}/componentType must be {expectedComponentType} for {pointer}.");
        }
    }

    private static bool TryGetAccessor(
        IReadOnlyList<AccessorDef> accessors,
        int accessorIndex,
        out AccessorDef accessor,
        out string pointer)
    {
        pointer = $"/accessors/{accessorIndex}";
        if (accessorIndex < 0 || accessorIndex >= accessors.Count)
        {
            accessor = null;
            return false;
        }

        accessor = accessors[accessorIndex];
        return true;
    }

    private static List<BufferDef> ParseBuffers(JsonElement root, Report report)
    {
        var result = new List<BufferDef>();
        if (!TryGetArray(root, "buffers", out var buffers))
        {
            return result;
        }

        for (int i = 0; i < buffers.GetArrayLength(); i++)
        {
            if (!TryGetRequiredInt(buffers[i], "byteLength", out int byteLength))
            {
                report.Errors.Add($"/buffers/{i}/byteLength is missing or invalid.");
                continue;
            }

            result.Add(new BufferDef(byteLength));
        }

        return result;
    }

    private static List<BufferViewDef> ParseBufferViews(JsonElement root, Report report)
    {
        var result = new List<BufferViewDef>();
        if (!TryGetArray(root, "bufferViews", out var views))
        {
            return result;
        }

        for (int i = 0; i < views.GetArrayLength(); i++)
        {
            var view = views[i];
            if (!TryGetRequiredInt(view, "buffer", out int buffer) ||
                !TryGetRequiredInt(view, "byteLength", out int byteLength))
            {
                report.Errors.Add($"/bufferViews/{i} is missing required buffer or byteLength.");
                continue;
            }

            int byteOffset = TryGetInt(view, "byteOffset", out int parsedOffset) ? parsedOffset : 0;
            int? byteStride = TryGetInt(view, "byteStride", out int parsedStride) ? parsedStride : null;
            int? target = TryGetInt(view, "target", out int parsedTarget) ? parsedTarget : null;

            result.Add(new BufferViewDef(buffer, byteOffset, byteLength, byteStride, target));
            report.Diagnostics.Add(
                $"[GLB.BUFFERVIEW] index={i} buffer={buffer} offset={byteOffset} length={byteLength} stride={(byteStride?.ToString() ?? "-")} target={(target?.ToString() ?? "-")}");
        }

        return result;
    }

    private static List<AccessorDef> ParseAccessors(JsonElement root, Report report)
    {
        var result = new List<AccessorDef>();
        if (!TryGetArray(root, "accessors", out var accessors))
        {
            return result;
        }

        for (int i = 0; i < accessors.GetArrayLength(); i++)
        {
            var accessor = accessors[i];
            if (!TryGetRequiredInt(accessor, "count", out int count) ||
                !TryGetRequiredInt(accessor, "componentType", out int componentType) ||
                !TryGetRequiredString(accessor, "type", out string type))
            {
                report.Errors.Add($"/accessors/{i} is missing required fields.");
                continue;
            }

            int? bufferView = TryGetInt(accessor, "bufferView", out int parsedView) ? parsedView : null;
            int byteOffset = TryGetInt(accessor, "byteOffset", out int parsedOffset) ? parsedOffset : 0;
            bool normalized = TryGetBool(accessor, "normalized", out bool parsedNormalized) && parsedNormalized;
            bool hasMin = accessor.TryGetProperty("min", out _);
            bool hasMax = accessor.TryGetProperty("max", out _);

            result.Add(new AccessorDef(bufferView, byteOffset, count, type, componentType, normalized, hasMin, hasMax));
            report.Diagnostics.Add(
                $"[GLB.ACCESSOR] index={i} view={(bufferView?.ToString() ?? "-")} offset={byteOffset} count={count} type={type} componentType={componentType}");
        }

        return result;
    }

    private static string DecodeJsonChunk(byte[] jsonChunk)
    {
        int trimEnd = jsonChunk.Length;
        while (trimEnd > 0 && jsonChunk[trimEnd - 1] == 0x20)
        {
            trimEnd--;
        }

        return Encoding.UTF8.GetString(jsonChunk, 0, trimEnd);
    }

    private static bool HasOnlyJsonPadding(byte[] jsonChunk)
    {
        if (jsonChunk.Length == 0)
        {
            return true;
        }

        int trimEnd = jsonChunk.Length;
        while (trimEnd > 0 && jsonChunk[trimEnd - 1] == 0x20)
        {
            trimEnd--;
        }

        for (int i = trimEnd; i < jsonChunk.Length; i++)
        {
            if (jsonChunk[i] != 0x20)
            {
                return false;
            }
        }

        return true;
    }

    private static int GetTypeComponentCount(string type) => type switch
    {
        "SCALAR" => 1,
        "VEC2" => 2,
        "VEC3" => 3,
        "VEC4" => 4,
        "MAT2" => 4,
        "MAT3" => 9,
        "MAT4" => 16,
        _ => -1
    };

    private static int GetComponentSize(int componentType) => componentType switch
    {
        ComponentByte => 1,
        ComponentUByte => 1,
        ComponentShort => 2,
        ComponentUShort => 2,
        ComponentUInt => 4,
        ComponentFloat => 4,
        _ => -1
    };

    private static bool IsStrictlyIncreasing(float[] values)
    {
        if (values.Length < 2)
        {
            return true;
        }

        float previous = values[0];
        if (!float.IsFinite(previous))
        {
            return false;
        }

        for (int i = 1; i < values.Length; i++)
        {
            float current = values[i];
            if (!float.IsFinite(current) || current <= previous)
            {
                return false;
            }

            previous = current;
        }

        return true;
    }

    private static bool IsArrayOfFiniteNumbers(JsonElement element, int expectedLength)
    {
        if (element.ValueKind != JsonValueKind.Array || element.GetArrayLength() != expectedLength)
        {
            return false;
        }

        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            if (!item.TryGetSingle(out float value) || !float.IsFinite(value))
            {
                return false;
            }
        }

        return true;
    }

    private static int GetArrayLength(JsonElement element, string propertyName)
    {
        return TryGetArray(element, propertyName, out var arr) ? arr.GetArrayLength() : 0;
    }

    private static bool TryGetArray(JsonElement element, string propertyName, out JsonElement arrayElement)
    {
        if (element.TryGetProperty(propertyName, out arrayElement) && arrayElement.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        arrayElement = default;
        return false;
    }

    private static bool TryGetObject(JsonElement element, string propertyName, out JsonElement objectElement)
    {
        if (element.TryGetProperty(propertyName, out objectElement) && objectElement.ValueKind == JsonValueKind.Object)
        {
            return true;
        }

        objectElement = default;
        return false;
    }

    private static bool TryGetInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var prop) &&
               prop.ValueKind == JsonValueKind.Number &&
               prop.TryGetInt32(out value);
    }

    private static bool TryGetRequiredInt(JsonElement element, string propertyName, out int value)
    {
        return TryGetInt(element, propertyName, out value);
    }

    private static bool TryGetBool(JsonElement element, string propertyName, out bool value)
    {
        value = false;
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            return false;
        }

        if (prop.ValueKind != JsonValueKind.True && prop.ValueKind != JsonValueKind.False)
        {
            return false;
        }

        value = prop.GetBoolean();
        return true;
    }

    private static bool TryGetRequiredString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = prop.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return prop.GetString();
    }
}
