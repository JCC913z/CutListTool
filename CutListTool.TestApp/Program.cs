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

bool proofLoadOnly = HasArgument(args, "--proof-load");
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

CutListInputData input = JsonSerializer.Deserialize<CutListInputData>(json, jsonOptions)
    ?? throw new InvalidOperationException($"Could not read cut list input data from {inputPath}.");

if (proofLoadOnly)
{
    PrintProofLoad(input);
    return;
}

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
                    Name: "All Cut Lists - Json",
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
    if (args.Length > 0 && File.Exists(args[0]))
    {
        return args[0];
    }

    string currentDirectoryPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "test-inputs.json"
    );

    if (File.Exists(currentDirectoryPath))
    {
        return currentDirectoryPath;
    }

    string outputDirectoryPath = Path.Combine(
        AppContext.BaseDirectory,
        "test-inputs.json"
    );

    if (File.Exists(outputDirectoryPath))
    {
        return outputDirectoryPath;
    }

    throw new FileNotFoundException(
        "Could not find test-inputs.json. Put it in the TestApp project folder, repo root, or pass the path as a command argument."
    );
}

static void PrintProofLoad(CutListInputData input)
{
    Dictionary<string, ConnectionType> connectionTypesByKey = input.ConnectionTypes
        .ToDictionary(
            connectionType => connectionType.Key,
            StringComparer.OrdinalIgnoreCase
        );

    Console.WriteLine("Loaded Connection Types");
    Console.WriteLine("============================");
    foreach (ConnectionType connectionType in input.ConnectionTypes)
    {
        string flangeNote = connectionType.UsesFlangeOptions
            ? " - uses flange options"
            : "";

        Console.WriteLine(
            $"{connectionType.Key} = {connectionType.DisplayName}{flangeNote}"
        );
    }

    Console.WriteLine();
    Console.WriteLine("Loaded Flex Connectors");
    Console.WriteLine("======================");

    foreach (FlexConnector flexConnector in input.FlexConnectors)
    {
        string connectionAText = GetConnectionText(
            flexConnector.ConnectionA,
            connectionTypesByKey
        );

        string connectionBText = GetConnectionText(
            flexConnector.ConnectionB,
            connectionTypesByKey
        );

        Console.WriteLine();
        Console.WriteLine(
            $"{flexConnector.Label} - {flexConnector.DimA}\" x {flexConnector.DimB}\" ({flexConnector.Qty}x)"
        );

        Console.WriteLine($"  {connectionAText} -> {connectionBText}");

        PrintConnectionSideDetails(
            "Connection A",
            flexConnector.ConnectionA,
            connectionTypesByKey
        );

        PrintConnectionSideDetails(
            "Connection B",
            flexConnector.ConnectionB,
            connectionTypesByKey
        );
    }
}

static string GetConnectionText(
    Connection connection,
    Dictionary<string, ConnectionType> connectionTypesByKey
)
{
    if (!connectionTypesByKey.TryGetValue(
        connection.ConnectionTypeKey,
        out ConnectionType? connectionType
    ))
    {
        return $"UNKNOWN CONNECTION TYPE: {connection.ConnectionTypeKey}";
    }
    else if (connection.SideConnections is not null && connection.SideConnections.Count > 0)
    {
        return $"Custom {connectionType.DisplayName}";
    }
    else if (connectionType.UsesFlangeOptions)
    {
        return GetFlangeText(
            connection.FlangeDirection,
            connection.FlangeSize
        );
    }
    else
    {
        return connectionType.DisplayName;
    }
}

static void PrintConnectionSideDetails(
    string connectionName,
    Connection connection,
    Dictionary<string, ConnectionType> connectionTypesByKey
)
{
    List<PerSideConnection> sideConnections =
        connection.SideConnections ?? new List<PerSideConnection>();

    if (sideConnections.Count == 0)
    {
        return;
    }

    Console.WriteLine($"  {connectionName} side details:");

    foreach (PerSideConnection sideConnection in sideConnections)
    {
        string sideText = GetSideConnectionText(
            sideConnection,
            connectionTypesByKey
        );

        Console.WriteLine($"    {sideConnection.Side}: {sideText}");
    }
}

static string GetSideConnectionText(
    PerSideConnection sideConnection,
    Dictionary<string, ConnectionType> connectionTypesByKey
)
{
    if (!connectionTypesByKey.TryGetValue(
        sideConnection.ConnectionTypeKey,
        out ConnectionType? connectionType
    ))
    {
        return $"UNKNOWN CONNECTION TYPE: {sideConnection.ConnectionTypeKey}";
    }
    else if (connectionType.UsesFlangeOptions)
    {
        return GetFlangeText(
            sideConnection.FlangeDirection,
            sideConnection.FlangeSize
        );
    }
    else
    {
        return connectionType.DisplayName;
    }
}

static string GetFlangeText(
    FlangeDirection? flangeDirection,
    decimal? flangeSize
)
{
    if (flangeDirection is null)
    {
        return "MISSING FLANGE DIRECTION";
    }
    else if (flangeDirection == FlangeDirection.Straight)
    {
        return "Straight";
    }
    else if (flangeSize is null)
    {
        return $"F{flangeDirection.Value.ToString()[0]} - MISSING FLANGE SIZE";
    }
    else
    {
        string flangeSizeText = flangeSize.Value.ToString("0.###");

        return $"{flangeSizeText}\" F{flangeDirection.Value.ToString()[0]}";
    }
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
    Console.WriteLine("  --proof-load      Print loaded connection/flex proof data only.");
    Console.WriteLine("  -h, --help        Show this help text.");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run");
    Console.WriteLine("  dotnet run -- --json");
    Console.WriteLine("  dotnet run -- --both");
    Console.WriteLine("  dotnet run -- test-inputs.json --json --out cut-list-output.json");
}

