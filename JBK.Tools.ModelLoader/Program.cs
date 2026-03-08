using JBK.Tools.ModelLoader;
using JBK.Tools.ModelLoader.Export;
using JBK.Tools.ModelLoader.Export.Glb;
using JBK.Tools.ModelLoader.FileReader;
using JBK.Tools.ModelLoader.Merge;
using System.CommandLine;
using System.CommandLine.Parsing;

var filenameOption = new Option<string>("--filename")
{
    Description = "Process one .gb file"
};

var pathOption = new Option<string>("--path")
{
    Description = "Process all .gb files in a folder"
};

var combineOption = new Option<bool>("--combine")
{
    Description = "Merge loaded files into one export (folder mode)"
};

var canonicalBoneOption = new Option<string>("--canonical-bone")
{
    Description = "Canonical skeleton file used to resolve multipart mesh/animation bone references"
};

var assumeBoneOrderOption = new Option<bool>("--assume-matching-bone-order")
{
    Description = "If canonical bone resolution fails, fall back to source->target bone index identity when the ordered bone hierarchy matches"
};

var exportOption = new Option<string>("--export")
{
    Description = "Export format: glb|obj",
    DefaultValueFactory = _ => "glb"
};

var texOption = new Option<string>("--tex")
{
    Description = "Texture folder override (default: <input>/tex)"
};

var outOption = new Option<string>("--out")
{
    Description = "Output directory or output file path"
};

var patternOption = new Option<string>("--pattern")
{
    Description = "File pattern in folder mode",
    DefaultValueFactory = _ => "*.gb"
};

var recursiveOption = new Option<bool>("--recursive")
{
    Description = "Search subdirectories in folder mode"
};

var failFastOption = new Option<bool>("--fail-fast")
{
    Description = "Stop on first error"
};

var verboseOption = new Option<bool>("--verbose")
{
    Description = "Verbose logging"
};

var exportDiagnosticsOption = new Option<bool>("--export-diagnostics")
{
    Description = "Print strict GLB conformance diagnostics after export"
};

var root = new RootCommand("JBK.Tools.ModelLoader CLI");
root.Add(filenameOption);
root.Add(pathOption);
root.Add(combineOption);
root.Add(canonicalBoneOption);
root.Add(assumeBoneOrderOption);
root.Add(exportOption);
root.Add(texOption);
root.Add(outOption);
root.Add(patternOption);
root.Add(recursiveOption);
root.Add(failFastOption);
root.Add(verboseOption);
root.Add(exportDiagnosticsOption);

root.Validators.Add(result =>
{
    var fileName = result.GetValue(filenameOption);
    var path = result.GetValue(pathOption);
    var export = result.GetValue(exportOption);
    var combine = result.GetValue(combineOption);
    var canonicalBone = result.GetValue(canonicalBoneOption);
    var assumeBoneOrder = result.GetValue(assumeBoneOrderOption);

    var hasFile = !string.IsNullOrWhiteSpace(fileName);
    var hasPath = !string.IsNullOrWhiteSpace(path);

    if (hasFile == hasPath)
    {
        result.AddError("Specify exactly one of --filename or --path.");
    }

    if (!string.Equals(export, "glb", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(export, "obj", StringComparison.OrdinalIgnoreCase))
    {
        result.AddError("Invalid --export value. Use glb or obj.");
    }

    if (combine && hasFile)
    {
        result.AddError("--combine is only meaningful with --path.");
    }

    if (!string.IsNullOrWhiteSpace(canonicalBone) && !hasPath && !hasFile)
    {
        result.AddError("--canonical-bone requires --filename or --path.");
    }

    if (assumeBoneOrder && string.IsNullOrWhiteSpace(canonicalBone))
    {
        result.AddError("--assume-matching-bone-order requires --canonical-bone.");
    }
});

root.SetAction(parseResult => Execute(
    parseResult,
    filenameOption,
    pathOption,
    combineOption,
    canonicalBoneOption,
    assumeBoneOrderOption,
    exportOption,
    texOption,
    outOption,
    patternOption,
    recursiveOption,
    failFastOption,
    verboseOption,
    exportDiagnosticsOption));

return root.Parse(args).Invoke();

static int Execute(
    ParseResult parseResult,
    Option<string> filenameOption,
    Option<string> pathOption,
    Option<bool> combineOption,
    Option<string> canonicalBoneOption,
    Option<bool> assumeBoneOrderOption,
    Option<string> exportOption,
    Option<string> texOption,
    Option<string> outOption,
    Option<string> patternOption,
    Option<bool> recursiveOption,
    Option<bool> failFastOption,
    Option<bool> verboseOption,
    Option<bool> exportDiagnosticsOption)
{
    var options = new CliOptions
    {
        FileName = parseResult.GetValue(filenameOption),
        Path = parseResult.GetValue(pathOption),
        Combine = parseResult.GetValue(combineOption),
        CanonicalBone = parseResult.GetValue(canonicalBoneOption),
        AssumeMatchingBoneOrder = parseResult.GetValue(assumeBoneOrderOption),
        ExportFormat = parseResult.GetValue(exportOption) ?? "glb",
        TexturePath = parseResult.GetValue(texOption),
        OutPath = parseResult.GetValue(outOption),
        Pattern = parseResult.GetValue(patternOption) ?? "*.gb",
        Recursive = parseResult.GetValue(recursiveOption),
        FailFast = parseResult.GetValue(failFastOption),
        Verbose = parseResult.GetValue(verboseOption),
        ExportDiagnostics = parseResult.GetValue(exportDiagnosticsOption)
    };

    if (!NormalizeAndValidatePaths(options, out var error))
    {
        Console.Error.WriteLine(error);
        return 1;
    }

    if (options.ExportFormat.Equals("obj", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("OBJ export is not implemented yet. Use --export glb.");
        return 1;
    }

    var exporter = CreateExporter(options.ExportFormat, options);
    var sourceFiles = ResolveSourceFiles(options);

    if (sourceFiles.Count == 0)
    {
        Console.Error.WriteLine("No .gb files found to process.");
        return 1;
    }

    return options.Combine
        ? ExportCombined(sourceFiles, options, exporter)
        : ExportIndividually(sourceFiles, options, exporter);
}

static bool NormalizeAndValidatePaths(CliOptions options, out string error)
{
    if (!string.IsNullOrWhiteSpace(options.FileName))
    {
        var fullPath = Path.GetFullPath(options.FileName);
        if (!File.Exists(fullPath))
        {
            error = $"Input file not found: {fullPath}";
            return false;
        }

        options.FileName = fullPath;
    }

    if (!string.IsNullOrWhiteSpace(options.Path))
    {
        var fullPath = Path.GetFullPath(options.Path);
        if (!Directory.Exists(fullPath))
        {
            error = $"Input directory not found: {fullPath}";
            return false;
        }

        options.Path = fullPath;
    }

    if (!string.IsNullOrWhiteSpace(options.TexturePath))
    {
        options.TexturePath = Path.GetFullPath(options.TexturePath);
    }

    if (!string.IsNullOrWhiteSpace(options.OutPath))
    {
        options.OutPath = Path.GetFullPath(options.OutPath);
    }

    if (!string.IsNullOrWhiteSpace(options.CanonicalBone))
    {
        if (!Path.IsPathRooted(options.CanonicalBone) && !string.IsNullOrWhiteSpace(options.Path))
        {
            options.CanonicalBone = Path.GetFullPath(Path.Combine(options.Path, options.CanonicalBone));
        }
        else
        {
            options.CanonicalBone = Path.GetFullPath(options.CanonicalBone);
        }

        if (!File.Exists(options.CanonicalBone))
        {
            error = $"Canonical bone file not found: {options.CanonicalBone}";
            return false;
        }
    }

    error = string.Empty;
    return true;
}

static int ExportCombined(IReadOnlyList<string> sourceFiles, CliOptions options, IExporter exporter)
{
    var orderedFiles = ResolveCombinedMergeOrder(sourceFiles, options);
    Model merged = null;
    bool resolveToCanonical = !string.IsNullOrWhiteSpace(options.CanonicalBone);
    int failures = 0;

    for (int i = 0; i < orderedFiles.Count; i++)
    {
        var file = orderedFiles[i];
        try
        {
            merged = merged is null
                ? GbFileLoader.LoadFromFile(file)
                : GbFileLoader.Append(
                    merged,
                    file,
                    new MergeOptions
                    {
                        ResolveBonesToTarget = resolveToCanonical,
                        AssumeMatchingBoneOrder = options.AssumeMatchingBoneOrder,
                        SourceLabel = file
                    });

            if (options.Verbose)
            {
                Console.WriteLine($"Loaded {file}");
            }
        }
        catch (Exception ex)
        {
            failures++;
            Console.Error.WriteLine($"Error loading {file}: {ex.Message}");
            if (options.FailFast)
            {
                return 1;
            }
        }
    }

    if (merged is null)
    {
        Console.Error.WriteLine("No valid input models could be loaded.");
        return 1;
    }

    if (failures > 0)
    {
        Console.Error.WriteLine($"Combined export aborted with {failures} failure(s).");
        return 1;
    }

    var extension = GetOutputExtension(options.ExportFormat);
    var outputPath = ResolveCombinedOutputPath(orderedFiles, options, extension);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

    var texturePath = ResolveTexturePath(options, orderedFiles[0]);
    exporter.Export(merged, texturePath, outputPath);
    Console.WriteLine($"Exported combined model -> {outputPath}");
    return 0;
}

static int ExportIndividually(IReadOnlyList<string> sourceFiles, CliOptions options, IExporter exporter)
{
    var failures = 0;
    var extension = GetOutputExtension(options.ExportFormat);
    bool resolveToCanonical = !string.IsNullOrWhiteSpace(options.CanonicalBone);
    string? canonicalPath = resolveToCanonical ? options.CanonicalBone : null;

    foreach (var file in sourceFiles)
    {
        try
        {
            if (resolveToCanonical
                && !string.IsNullOrWhiteSpace(options.Path)
                && string.Equals(file, canonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                if (options.Verbose)
                {
                    Console.WriteLine($"Skipped canonical source file {file}");
                }

                continue;
            }

            Model model;
            if (resolveToCanonical && !string.Equals(file, canonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                model = GbFileLoader.LoadFromFile(canonicalPath!);
                model = GbFileLoader.Append(
                    model,
                    file,
                    new MergeOptions
                    {
                        ResolveBonesToTarget = true,
                        AssumeMatchingBoneOrder = options.AssumeMatchingBoneOrder,
                        SourceLabel = file
                    });
            }
            else
            {
                model = GbFileLoader.LoadFromFile(file);
            }

            var outputPath = ResolvePerFileOutputPath(file, options, extension);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var texturePath = ResolveTexturePath(options, file);
            exporter.Export(model, texturePath, outputPath);
            Console.WriteLine($"Exported {Path.GetFileName(file)} -> {outputPath}");
        }
        catch (Exception ex)
        {
            failures++;
            Console.Error.WriteLine($"Error processing {file}: {ex.Message}");
            if (options.FailFast)
            {
                return 1;
            }
        }
    }

    if (failures > 0)
    {
        Console.Error.WriteLine($"Completed with {failures} failure(s).");
        return 1;
    }

    return 0;
}

static IExporter CreateExporter(string format, CliOptions options)
{
    return format.ToLowerInvariant() switch
    {
        "glb" => new GlbExporter(new GlbExporterOptions { ExportDiagnostics = options.ExportDiagnostics }),
        _ => throw new NotSupportedException($"Unsupported export format '{format}'.")
    };
}

static List<string> ResolveSourceFiles(CliOptions options)
{
    if (!string.IsNullOrWhiteSpace(options.FileName))
    {
        return new List<string> { options.FileName };
    }

    var searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    return Directory
        .GetFiles(options.Path, options.Pattern, searchOption)
        .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

static string ResolveTexturePath(CliOptions options, string sourceFile)
{
    if (!string.IsNullOrWhiteSpace(options.TexturePath))
    {
        return options.TexturePath;
    }

    var sourceDir = Path.GetDirectoryName(Path.GetFullPath(sourceFile))!;
    return Path.Combine(sourceDir, "tex");
}

static string ResolvePerFileOutputPath(string sourceFile, CliOptions options, string extension)
{
    if (string.IsNullOrWhiteSpace(options.OutPath))
    {
        return Path.ChangeExtension(Path.GetFullPath(sourceFile), extension);
    }

    if (HasKnownFileExtension(options.OutPath))
    {
        return options.OutPath;
    }

    var fileName = Path.GetFileNameWithoutExtension(sourceFile) + extension;
    return Path.Combine(options.OutPath, fileName);
}

static string ResolveCombinedOutputPath(IReadOnlyList<string> sourceFiles, CliOptions options, string extension)
{
    if (!string.IsNullOrWhiteSpace(options.OutPath))
    {
        if (HasKnownFileExtension(options.OutPath))
        {
            return options.OutPath;
        }

        return Path.Combine(options.OutPath, "combined" + extension);
    }

    var baseFolder = !string.IsNullOrWhiteSpace(options.Path)
        ? options.Path
        : Path.GetDirectoryName(Path.GetFullPath(sourceFiles[0]))!;

    return Path.Combine(baseFolder, "combined" + extension);
}

static List<string> ResolveCombinedMergeOrder(IReadOnlyList<string> sourceFiles, CliOptions options)
{
    if (string.IsNullOrWhiteSpace(options.CanonicalBone))
    {
        return sourceFiles.ToList();
    }

    var canonicalPath = options.CanonicalBone;
    var ordered = new List<string> { canonicalPath };

    for (int i = 0; i < sourceFiles.Count; i++)
    {
        var file = sourceFiles[i];
        if (string.Equals(file, canonicalPath, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        ordered.Add(file);
    }

    return ordered;
}

static bool HasKnownFileExtension(string path)
{
    var ext = Path.GetExtension(path);
    return ext.Equals(".glb", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".obj", StringComparison.OrdinalIgnoreCase);
}

static string GetOutputExtension(string format)
{
    return format.ToLowerInvariant() switch
    {
        "glb" => ".glb",
        "obj" => ".obj",
        _ => throw new NotSupportedException($"Unsupported export format '{format}'.")
    };
}

sealed class CliOptions
{
    public string FileName { get; set; }
    public string Path { get; set; }
    public bool Combine { get; set; }
    public string CanonicalBone { get; set; }
    public bool AssumeMatchingBoneOrder { get; set; }
    public string ExportFormat { get; set; } = "glb";
    public string TexturePath { get; set; }
    public string OutPath { get; set; }
    public string Pattern { get; set; } = "*.gb";
    public bool Recursive { get; set; }
    public bool FailFast { get; set; }
    public bool Verbose { get; set; }
    public bool ExportDiagnostics { get; set; }
}
