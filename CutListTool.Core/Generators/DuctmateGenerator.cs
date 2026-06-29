using CutListTool.Core.Models;
using CutListTool.Core.Services;
using CutListTool.Core.Settings;

namespace CutListTool.Core.Generators;

public class DuctmateGenerator
{
    private readonly UserPreferences prefs;
    public DuctmateGenerator(UserPreferences preferences) {this.prefs = preferences;}

    public GeneratedBuildOutput Generate(DuctmateFrame dmFrame)
    {
        BuildListLine buildLine = new(
            BuildType: BuildItemType.Ductmate,
            Text: GetBuildListText(dmFrame)
        );

        List<LinearCutItem> linearCuts = new()
        {
            new(
                Length: dmFrame.Width - prefs.DMCutAllowance,
                Qty: dmFrame.Qty * 2,
                BuildType: BuildItemType.Ductmate,
                CutType: CutItemType.Ductmate
            ),

            new(
                Length: dmFrame.Height - prefs.DMCutAllowance,
                Qty: dmFrame.Qty * 2,
                BuildType: BuildItemType.Ductmate,
                CutType: CutItemType.Ductmate
            )
        };

        List<CountCutItem> countCuts = new();

        return new GeneratedBuildOutput(
            BuildLine: buildLine,
            LinearCuts: linearCuts,
            CountCuts: countCuts
        );
    }

    private string GetBuildListText(DuctmateFrame dmFrame)
    {
        string labelText = string.IsNullOrWhiteSpace(dmFrame.Label)
            ? ""
            : $" - {dmFrame.Label}";

        return $"{dmFrame.Qty}x) {MathJC.RoundToSixteenth(dmFrame.Width)}\" x {MathJC.RoundToSixteenth(dmFrame.Height)}\"{labelText}";
    }
}
