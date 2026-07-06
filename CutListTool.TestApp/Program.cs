using CutListTool.Core.Generators;
using CutListTool.Core.Models;
using CutListTool.Core.Models.Outputs;
using CutListTool.Core.Services;
using CutListTool.Core.Services.Outputs;
using CutListTool.Core.Settings;
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

bool proofLoadOnly = false;

//Initialization
UserPreferences prefs = new();

List<BuildListLine> buildLines = [];
List<LinearCutItem> rawLinearCuts = [];
List<CountCutItem> rawCountCuts = [];

JsonSerializerOptions jsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};

jsonOptions.Converters.Add(new JsonStringEnumConverter());

string inputPath = GetInputPath(args);
string json = File.ReadAllText(inputPath);

TestInputData input = JsonSerializer.Deserialize<TestInputData>(json, jsonOptions) ?? throw new InvalidOperationException($"Could not read test input data from {inputPath}.");

DuctmateGenerator dmGenerator = new(prefs);
LinerGenerator linerGenerator = new(prefs);
TurnVaneGenerator turnVaneGenerator = new(prefs);
FlexConnectorGenerator flexConnectorGenerator = new(input.ConnectionTypes, prefs);

if (proofLoadOnly)
{
    PrintProofLoad(input);
    return;
}

//Gather and Sort Data
foreach (DuctmateFrame dmFrame in input.DuctmateFrames)
{
    GeneratedBuildOutput output = dmGenerator.Generate(dmFrame);

    buildLines.Add(output.BuildLine);
    rawLinearCuts.AddRange(output.LinearCuts);
    rawCountCuts.AddRange(output.CountCuts);
}

foreach (Liner liner in input.Liners)
{
    GeneratedBuildOutput output = linerGenerator.Generate(liner);

    buildLines.Add(output.BuildLine);
    rawLinearCuts.AddRange(output.LinearCuts);
    rawCountCuts.AddRange(output.CountCuts);
}

foreach (TurnVane turnVane in input.TurnVanes)
{
    GeneratedBuildOutput output = turnVaneGenerator.Generate(turnVane);

    buildLines.Add(output.BuildLine);
    rawLinearCuts.AddRange(output.LinearCuts);
    rawCountCuts.AddRange(output.CountCuts);
}

foreach (FlexConnector flexConnector in input.FlexConnectors)
{
    GeneratedBuildOutput output = flexConnectorGenerator.Generate(flexConnector);

    buildLines.Add(output.BuildLine);
    rawLinearCuts.AddRange(output.LinearCuts);
    rawCountCuts.AddRange(output.CountCuts);
}

List<LinearCutItem> groupedLinearCuts = CutListGrouper.GroupLinearCuts(rawLinearCuts);
List<CountCutItem> groupedCountCuts = CutListGrouper.GroupCountCuts(rawCountCuts);
CutListOutputData cutListOutputData = CutListOutputBuilder.Build(
    buildLines,
    groupedLinearCuts,
    groupedCountCuts
);

List<BuildItemType> buildTypes = buildLines
    .Select(line => line.BuildType)
    .Union(groupedLinearCuts.Select(cut => cut.BuildType))
    .Union(groupedCountCuts.Select(cut => cut.BuildType))
    .Distinct()
    .OrderBy(type => type)
    .ToList()
;


//Output Processes
TextCutListOutputService textOutputService = new();

string textOutput = textOutputService.Generate(cutListOutputData);

Console.Write(textOutput);

JsonCutListOutputService jsonOutputService = new();

string jsonOutput = jsonOutputService.Generate(cutListOutputData);

File.WriteAllText("cut-list-output.json", jsonOutput);


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

static void PrintProofLoad(TestInputData input)
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

public sealed class TestInputData
{
    public List<DuctmateFrame> DuctmateFrames { get; init; } = new();
    public List<Liner> Liners { get; init; } = new();
    public List<TurnVane> TurnVanes { get; init; } = new();

    public List<ConnectionType> ConnectionTypes { get; init; } = new();
    public List<FlexConnector> FlexConnectors { get; init; } = new();
}

