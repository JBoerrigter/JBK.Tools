using JBK.Tools.ModelLoader;
using JBK.Tools.ModelLoader.Export;
using JBK.Tools.ModelLoader.Export.Glb;
using JBK.Tools.ModelLoader.FileReader;
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

var root = new RootCommand("JBK.Tools.ModelLoader CLI");
root.Add(filenameOption);
root.Add(pathOption);
root.Add(combineOption);
root.Add(exportOption);
root.Add(texOption);
root.Add(outOption);
root.Add(patternOption);
root.Add(recursiveOption);
root.Add(failFastOption);
root.Add(verboseOption);

root.Validators.Add(result =>
{
    var fileName = result.GetValue(filenameOption);
    var path = result.GetValue(pathOption);
    var export = result.GetValue(exportOption);
    var combine = result.GetValue(combineOption);

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
});

root.SetAction(parseResult => Execute(
    parseResult,
    filenameOption,
    pathOption,
    combineOption,
    exportOption,
    texOption,
    outOption,
    patternOption,
    recursiveOption,
    failFastOption,
    verboseOption));

return root.Parse(args).Invoke();

static int Execute(
    ParseResult parseResult,
    Option<string> filenameOption,
    Option<string> pathOption,
    Option<bool> combineOption,
    Option<string> exportOption,
    Option<string> texOption,
    Option<string> outOption,
    Option<string> patternOption,
    Option<bool> recursiveOption,
    Option<bool> failFastOption,
    Option<bool> verboseOption)
{
    var options = new CliOptions
    {
        FileName = parseResult.GetValue(filenameOption),
        Path = parseResult.GetValue(pathOption),
        Combine = parseResult.GetValue(combineOption),
        ExportFormat = parseResult.GetValue(exportOption) ?? "glb",
        TexturePath = parseResult.GetValue(texOption),
        OutPath = parseResult.GetValue(outOption),
        Pattern = parseResult.GetValue(patternOption) ?? "*.gb",
        Recursive = parseResult.GetValue(recursiveOption),
        FailFast = parseResult.GetValue(failFastOption),
        Verbose = parseResult.GetValue(verboseOption)
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

    var exporter = CreateExporter(options.ExportFormat);
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

    error = string.Empty;
    return true;
}

static int ExportCombined(IReadOnlyList<string> sourceFiles, CliOptions options, IExporter exporter)
{
    Model merged = null;

    foreach (var file in sourceFiles)
    {
        try
        {
            merged = merged is null
                ? GbFileLoader.LoadFromFile(file)
                : GbFileLoader.Append(merged, file);

            if (options.Verbose)
            {
                Console.WriteLine($"Loaded {file}");
            }
        }
        catch (Exception ex)
        {
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

    var extension = GetOutputExtension(options.ExportFormat);
    var outputPath = ResolveCombinedOutputPath(sourceFiles, options, extension);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

    var texturePath = ResolveTexturePath(options, sourceFiles[0]);
    exporter.Export(merged, texturePath, outputPath);
    Console.WriteLine($"Exported combined model -> {outputPath}");
    return 0;
}

static int ExportIndividually(IReadOnlyList<string> sourceFiles, CliOptions options, IExporter exporter)
{
    var failures = 0;
    var extension = GetOutputExtension(options.ExportFormat);

    foreach (var file in sourceFiles)
    {
        try
        {
            var model = GbFileLoader.LoadFromFile(file);
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

static IExporter CreateExporter(string format)
{
    return format.ToLowerInvariant() switch
    {
        "glb" => new GlbExporter(),
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
    public string ExportFormat { get; set; } = "glb";
    public string TexturePath { get; set; }
    public string OutPath { get; set; }
    public string Pattern { get; set; } = "*.gb";
    public bool Recursive { get; set; }
    public bool FailFast { get; set; }
    public bool Verbose { get; set; }
}
