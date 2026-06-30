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

public sealed class TestInputData
{
    public List<DuctmateFrame> DuctmateFrames { get; init; } = new();
    public List<Liner> Liners { get; init; } = new();
    public List<TurnVane> TurnVanes { get; init; } = new();
}
