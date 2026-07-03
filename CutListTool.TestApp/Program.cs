using CutListTool.Core.Generators;
using CutListTool.Core.Models;
using CutListTool.Core.Services;
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

//Initialization
UserPreferences prefs = new();

List<BuildListLine> buildLines = new();
List<LinearCutItem> rawLinearCuts = new();
List<CountCutItem> rawCountCuts = new();

DuctmateGenerator dmGenerator = new(prefs);
LinerGenerator linerGenerator = new(prefs);
TurnVaneGenerator turnVaneGenerator = new(prefs);

JsonSerializerOptions jsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true
};

jsonOptions.Converters.Add(new JsonStringEnumConverter());

string inputPath = GetInputPath(args);
string json = File.ReadAllText(inputPath);

TestInputData input = JsonSerializer.Deserialize<TestInputData>(json, jsonOptions)
    ?? throw new InvalidOperationException($"Could not read test input data from {inputPath}.");

bool proofLoadOnly = true;
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

List<LinearCutItem> groupedLinearCuts = CutListGrouper.GroupLinearCuts(rawLinearCuts);
List<CountCutItem> groupedCountCuts = CutListGrouper.GroupCountCuts(rawCountCuts);

List<BuildItemType> buildTypes = buildLines
    .Select(line => line.BuildType)
    .Union(groupedLinearCuts.Select(cut => cut.BuildType))
    .Union(groupedCountCuts.Select(cut => cut.BuildType))
    .Distinct()
    .OrderBy(type => type)
    .ToList()
;



//Output Processes
foreach (BuildItemType buildType in buildTypes)
{
    Console.WriteLine(buildType.ToString().ToUpper());
    Console.WriteLine("==========");
    Console.WriteLine();

    Console.WriteLine("Build List");
    Console.WriteLine("----------");

    List<BuildListLine> matchingBuildLines = buildLines
        .Where(line => line.BuildType == buildType)
        .ToList()
    ;

    foreach (BuildListLine buildLine in matchingBuildLines)
    {
        Console.WriteLine(buildLine.Text);
    }

    Console.WriteLine();

    Console.WriteLine("Cut List");
    Console.WriteLine("--------");

    List<LinearCutItem> matchingLinearCuts = groupedLinearCuts
        .Where(cut => cut.BuildType == buildType)
        .ToList()
    ;

    List<CountCutItem> matchingCountCuts = groupedCountCuts
        .Where(cut => cut.BuildType == buildType)
        .ToList()
    ;

    List<CutItemType> cutTypes = matchingLinearCuts
        .Select(cut => cut.CutType)
        .Union(matchingCountCuts.Select(cut => cut.CutType))
        .Distinct()
        .OrderBy(cutType => cutType)
        .ToList()
    ;

    foreach (CutItemType cutType in cutTypes)
    {
        Console.WriteLine();
        Console.WriteLine(cutType);
        Console.WriteLine("----------");

        List<LinearCutItem> linearCutsForCutType = matchingLinearCuts
            .Where(cut => cut.CutType == cutType)
            .ToList()
        ;

        List<CountCutItem> countCutsForCutType = matchingCountCuts
            .Where(cut => cut.CutType == cutType)
            .ToList()
        ;

        List<string?> groupLabels = linearCutsForCutType
            .Select(cut => cut.GroupLabel)
            .Union(countCutsForCutType.Select(cut => cut.GroupLabel))
            .Distinct()
            .OrderBy(groupLabel => groupLabel)
            .ToList();

        foreach (string? groupLabel in groupLabels)
        {
            if (!string.IsNullOrWhiteSpace(groupLabel))
            {
                Console.WriteLine();
                Console.WriteLine(groupLabel);
                Console.WriteLine("----------");
            }

            List<LinearCutItem> linearCutsInGroup = linearCutsForCutType
                .Where(cut => cut.GroupLabel == groupLabel)
                .ToList();

            foreach (LinearCutItem cutItem in linearCutsInGroup)
            {
                string displayLength = cutItem.Length.ToString();
                if (cutItem.DisplayInSixteenths) { displayLength = MathJC.RoundToSixteenth(cutItem.Length); }
                Console.WriteLine($"{cutItem.Qty} @ {displayLength}\"");
            }

            List<CountCutItem> countCutsInGroup = countCutsForCutType
                .Where(cut => cut.GroupLabel == groupLabel)
                .ToList();

            foreach (CountCutItem cutItem in countCutsInGroup)
            {
                if (cutItem.CutType == CutItemType.TV_Rails)
                {
                    Console.WriteLine($"{cutItem.Qty} @ {cutItem.CountSize}-vane rail");
                }
                else
                {
                    Console.WriteLine($"{cutItem.Qty} @ {cutItem.CountSize}");
                }
            }
        }
    }
    Console.WriteLine();
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

static void PrintProofLoad(TestInputData input)
{
    Dictionary<string, FlexConnectionType> connectionTypesByKey = input.FlexConnectionTypes
        .ToDictionary(
            connectionType => connectionType.Key,
            StringComparer.OrdinalIgnoreCase
        );

    Console.WriteLine("Loaded Flex Connection Types");
    Console.WriteLine("============================");
    foreach (FlexConnectionType connectionType in input.FlexConnectionTypes)
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
        string connectionAText = GetFlexConnectionText(
            flexConnector.ConnectionA,
            connectionTypesByKey
        );

        string connectionBText = GetFlexConnectionText(
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

static string GetFlexConnectionText(
    FlexConnection connection,
    Dictionary<string, FlexConnectionType> connectionTypesByKey
)
{
    if (!connectionTypesByKey.TryGetValue(
        connection.ConnectionTypeKey,
        out FlexConnectionType? connectionType
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
    FlexConnection connection,
    Dictionary<string, FlexConnectionType> connectionTypesByKey
)
{
    List<FlexSideConnection> sideConnections =
        connection.SideConnections ?? new List<FlexSideConnection>();

    if (sideConnections.Count == 0)
    {
        return;
    }

    Console.WriteLine($"  {connectionName} side details:");

    foreach (FlexSideConnection sideConnection in sideConnections)
    {
        string sideText = GetSideConnectionText(
            sideConnection,
            connectionTypesByKey
        );

        Console.WriteLine($"    {sideConnection.Side}: {sideText}");
    }
}

static string GetSideConnectionText(
    FlexSideConnection sideConnection,
    Dictionary<string, FlexConnectionType> connectionTypesByKey
)
{
    if (!connectionTypesByKey.TryGetValue(
        sideConnection.ConnectionTypeKey,
        out FlexConnectionType? connectionType
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

    public List<FlexConnectionType> FlexConnectionTypes { get; init; } = new();
    public List<FlexConnector> FlexConnectors { get; init; } = new();
}

