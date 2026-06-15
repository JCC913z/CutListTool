using CutListTool.Core.Generators;
using CutListTool.Core.Models;
using CutListTool.Core.Services;


//Initialization
List<IBuildItem> buildItems = new();
DuctmateGenerator dmGenerator = new();
LinerGenerator linerGenerator = new();

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

List<LinearCutItem> rawCuts = new();


//Gather and Sort Data
foreach (DuctmateFrame dmFrame in dmFrames)
{
    buildItems.Add(dmFrame);

    List<LinearCutItem> frameCuts = dmGenerator.Generate(dmFrame);
    rawCuts.AddRange(frameCuts);
}

foreach (Liner liner in liners)
{
    buildItems.Add(liner);

    List<LinearCutItem> linerCuts = linerGenerator.Generate(liner);
    rawCuts.AddRange(linerCuts);
}

List<LinearCutItem> groupedCuts = CutListGrouper.GroupLinearCuts(rawCuts);

List<CutItemType> cutTypes = groupedCuts.Select(cut => cut.Type).Distinct().OrderBy(type => type).ToList();


//Output Processes
foreach (CutItemType cutType in cutTypes)
{
    Console.WriteLine(cutType.ToString().ToUpper());
    Console.WriteLine("==========");
    Console.WriteLine();

    Console.WriteLine("Build List");
    Console.WriteLine("----------");

    List<IBuildItem> matchingBuildItems = buildItems.Where(item => item.Type == cutType).ToList();

    foreach (IBuildItem buildItem in matchingBuildItems)
    {
        Console.WriteLine(buildItem.GetBuildListText());
    }

    Console.WriteLine();

    Console.WriteLine("Cut List");
    Console.WriteLine("--------");

    List<LinearCutItem> matchingCuts = groupedCuts.Where(cut => cut.Type == cutType).ToList();
    
    List<string?> groupLabels = matchingCuts.Select(cut => cut.GroupLabel).Distinct().OrderBy(groupLabel => groupLabel).ToList();

    foreach (string? groupLabel in groupLabels)
    {
        if (!string.IsNullOrWhiteSpace(groupLabel))
        {
            Console.WriteLine();
            Console.WriteLine(groupLabel);
            Console.WriteLine("----------");
        }

        List<LinearCutItem> cutsInGroup = matchingCuts.Where(cut => cut.GroupLabel == groupLabel).ToList();

        foreach (LinearCutItem cutItem in cutsInGroup)
        {
            Console.WriteLine($"{cutItem.Qty} @ {cutItem.Length}\"");
        }
    }

    Console.WriteLine();
}
