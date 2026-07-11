using CutListTool.Core.Models;
using CutListTool.Core.Models.OutsideDataHandlers;
using CutListTool.Core.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

//Testing Area
bool testing = false;
if (testing)
{
    //Write Test Here and set bool to true, set back to false when done
    Console.WriteLine(MathJC.RoundToSixteenth(23.99m));    

    //End Test Area
    return;
}

if (HasArgument(args, "--help") || HasArgument(args, "-h"))
{
    PrintHelp();
    return;
}

OutputMode outputMode = GetOutputMode(args);
string jsonOutputPath = GetJsonOutputPath(args);


//Initialization
JsonSerializerOptions jsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};

jsonOptions.Converters.Add(new JsonStringEnumConverter());

string inputPath = GetInputPath(args);
string json = File.ReadAllText(inputPath);

CutListInputData input = JsonSerializer.Deserialize<CutListInputData>(json, jsonOptions) ?? throw new InvalidOperationException($"Could not read cut list input data from {inputPath}.");

CutListRequest request = BuildCutListRequest(outputMode);

CutListResult result = CutListEngine.Generate(input, request);

//Output Processes
foreach (CutListPackage package in result.Packages)
{
    if (package.OutputFormat == CutListOutputFormat.Text)
    {
        Console.Write(package.RenderedOutput);
    }
    else if (package.OutputFormat == CutListOutputFormat.Json)
    {
        File.WriteAllText(jsonOutputPath, package.RenderedOutput);

        Console.WriteLine($"JSON output written to: {jsonOutputPath}");
    }
}

static CutListRequest BuildCutListRequest(OutputMode outputMode)
{
    List<BuildItemType> allBuildTypes = Enum.GetValues<BuildItemType>().ToList();

    if (outputMode == OutputMode.Both)
    {
        return new CutListRequest(
            Packages:
            [
                new CutListPackageRequest(
                    Name: "All Cut Lists - Text",
                    IncludedBuildTypes: allBuildTypes,
                    OutputFormat: CutListOutputFormat.Text
                ),

                new CutListPackageRequest(
                    Name: "All Cut Lists - JSON",
                    IncludedBuildTypes: allBuildTypes,
                    OutputFormat: CutListOutputFormat.Json
                )
            ]
        );
    }

    CutListOutputFormat outputFormat = outputMode switch
    {
        OutputMode.Text => CutListOutputFormat.Text,
        OutputMode.Json => CutListOutputFormat.Json,
        _ => throw new ArgumentOutOfRangeException(nameof(outputMode), outputMode, null)
    };

    return new CutListRequest(
        Packages:
        [
            new CutListPackageRequest(
                Name: "All Cut Lists",
                IncludedBuildTypes: allBuildTypes,
                OutputFormat: outputFormat
            )
        ]
    );
}

static string GetInputPath(string[] args)
{
    string? suppliedInputPath = args
        .FirstOrDefault(arg => !arg.StartsWith('-') && File.Exists(arg));

    if (suppliedInputPath is not null)
    {
        return suppliedInputPath;
    }

    string testAppRelativePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "CutListTool.TestApp",
        "TestInputs",
        "basic-all.json"
    );

    if (File.Exists(testAppRelativePath))
    {
        return testAppRelativePath;
    }

    string currentDirectoryPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "TestInputs",
        "basic-all.json"
    );

    if (File.Exists(currentDirectoryPath))
    {
        return currentDirectoryPath;
    }

    string outputDirectoryPath = Path.Combine(
        AppContext.BaseDirectory,
        "TestInputs",
        "basic-all.json"
    );

    if (File.Exists(outputDirectoryPath))
    {
        return outputDirectoryPath;
    }

    throw new FileNotFoundException(
        "Could not find a test input file. Default expected path: CutListTool.TestApp/TestInputs/basic-all.json. You can also pass a specific input file path as a command argument."
    );
}

static bool HasArgument(string[] args, string argumentName)
{
    return args.Any(arg =>
        string.Equals(
            arg,
            argumentName,
            StringComparison.OrdinalIgnoreCase
        )
    );
}

static OutputMode GetOutputMode(string[] args)
{
    bool textRequested = HasArgument(args, "--text");
    bool jsonRequested = HasArgument(args, "--json");
    bool bothRequested = HasArgument(args, "--both");

    if (bothRequested || (textRequested && jsonRequested))
    {
        return OutputMode.Both;
    }

    if (jsonRequested)
    {
        return OutputMode.Json;
    }

    return OutputMode.Text;
}

static string GetJsonOutputPath(string[] args)
{
    string? outputPath = GetArgumentValue(args, "--out");

    if (!string.IsNullOrWhiteSpace(outputPath))
    {
        return outputPath;
    }

    return "cut-list-output.json";
}

static string? GetArgumentValue(string[] args, string argumentName)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(
            args[i],
            argumentName,
            StringComparison.OrdinalIgnoreCase
        ))
        {
            return args[i + 1];
        }
    }

    return null;
}

static void PrintHelp()
{
    Console.WriteLine("CutListTool TestApp");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run");
    Console.WriteLine("  dotnet run -- [input-file] [options]");
    Console.WriteLine();
    Console.WriteLine("Input:");
    Console.WriteLine("  input-file        Optional path to a JSON input file.");
    Console.WriteLine("                    If omitted, the app looks for test-inputs.json.");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --text            Print text cut list only. This is the default.");
    Console.WriteLine("  --json            Write JSON output only.");
    Console.WriteLine("  --both            Print text output and write JSON output.");
    Console.WriteLine("  --out <file>      Set JSON output file path. Default: cut-list-output.json");
    Console.WriteLine("  -h, --help        Show this help text.");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run");
    Console.WriteLine("  dotnet run -- --json");
    Console.WriteLine("  dotnet run -- --both");
    Console.WriteLine("  dotnet run -- test-inputs.json --json --out cut-list-output.json");
}

