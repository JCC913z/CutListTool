using CutListTool.Core.Generators;
using CutListTool.Core.Models;
using CutListTool.Core.Services;
using CutListTool.Core.Settings;

//Initialization
UserPreferences prefs = new();

List<BuildListLine> buildLines = new();
List<LinearCutItem> rawLinearCuts = new();
List<CountCutItem> rawCountCuts = new();

DuctmateGenerator dmGenerator = new(prefs);
LinerGenerator linerGenerator = new(prefs);
TurnVaneGenerator turnVaneGenerator = new(prefs);

List<DuctmateFrame> dmFrames = new()
{
    new(Width: 24, Height: 12, Qty: 1, Label: "AHU-1"),
    new(Width: 36, Height: 12, Qty: 1, Label: "AHU-2"),
    new(Width: 24, Height: 12, Qty: 2, Label: "RTU-1")
};

List<Liner> liners = new()
{
    new(
        Width: 48,
        Height: 36,
        Qty: 1,
        RollLength: LinerRollLength.Roll59,
        Thickness: LinerThickness.One_Inch,
        PieceMode: LinerPieceMode.FourPiece,
        Label: "AHU-1"
    ),

    new(
        Width: 24,
        Height: 12,
        Qty: 1,
        RollLength: LinerRollLength.Roll56,
        Thickness: LinerThickness.One_Inch,
        PieceMode: LinerPieceMode.TwoPiece,
        Label: "AHU-2"
    ),

    new(
        Width: 48,
        Height: 36,
        Qty: 1,
        RollLength: LinerRollLength.Roll59,
        Thickness: LinerThickness.OneAndHalf_Inch,
        PieceMode: LinerPieceMode.FourPiece,
        Label: "RTU-1"
    )
};

List<TurnVane> turnVanes = new()
{
    new(
        CheekA: 24,
        CheekB: 18,
        Heel: 12,
        Liner: LinerThickness.None,
        Qty: 1,
        Label: "AHU-1"
    ),

    new(
        CheekA: 36,
        CheekB: 24,
        Heel: 16,
        Liner: LinerThickness.One_Inch,
        Qty: 2,
        Label: "RTU-1"
    ),

    new(36, 36, 48, LinerThickness.One_Inch, 3, "#15", 2)
};



//Gather and Sort Data
foreach (DuctmateFrame dmFrame in dmFrames)
{
    GeneratedBuildOutput output = dmGenerator.Generate(dmFrame);

    buildLines.Add(output.BuildLine);
    rawLinearCuts.AddRange(output.LinearCuts);
    rawCountCuts.AddRange(output.CountCuts);
}

foreach (Liner liner in liners)
{
    GeneratedBuildOutput output = linerGenerator.Generate(liner);

    buildLines.Add(output.BuildLine);
    rawLinearCuts.AddRange(output.LinearCuts);
    rawCountCuts.AddRange(output.CountCuts);
}

foreach (TurnVane turnVane in turnVanes)
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
                Console.WriteLine($"{cutItem.Qty} @ {cutItem.Length}\"");
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

