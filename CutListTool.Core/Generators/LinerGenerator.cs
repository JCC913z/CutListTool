using CutListTool.Core.Settings;
using CutListTool.Core.Models;
using CutListTool.Core.Services;

namespace CutListTool.Core.Generators;

public class LinerGenerator
{
    private readonly UserPreferences prefs;
    public LinerGenerator(UserPreferences preferences) {this.prefs = preferences;}

    public GeneratedBuildOutput Generate(Liner liner)
    {
        string groupLabel = GetGroupLabel(liner);

        BuildListLine buildLine = new(
            BuildType: BuildItemType.Liner,
            Text: GetBuildListText(liner)
        );

        List<LinearCutItem> linearCuts;

        if (liner.PieceMode == LinerPieceMode.FourPiece)
        {
            linearCuts = GenerateFourPieceCuts(liner, groupLabel);
        }
        else
        {
            linearCuts = GenerateTwoPieceCuts(liner, groupLabel);
        }

        List<CountCutItem> countCuts = new();

        return new GeneratedBuildOutput(
            BuildLine: buildLine,
            LinearCuts: linearCuts,
            CountCuts: countCuts
        );
    }

    private List<LinearCutItem> GenerateFourPieceCuts(Liner liner, string groupLabel)
    {
        return new List<LinearCutItem>
        {
            new(
                Length: liner.Width - prefs.FourPieceWidthDeduction,
                Qty: liner.Qty * 2,
                BuildType: BuildItemType.Liner,
                CutType: CutItemType.Liner,
                GroupLabel: groupLabel
            ),

            new(
                Length: liner.Height - prefs.FourPieceHeightDeduction,
                Qty: liner.Qty * 2,
                BuildType: BuildItemType.Liner,
                CutType: CutItemType.Liner,
                GroupLabel: groupLabel
            )
        };
    }

    private List<LinearCutItem> GenerateTwoPieceCuts(Liner liner, string groupLabel)
    {
        decimal cutLength = liner.Width + liner.Height - prefs.TwoPieceDeduction;

        return new List<LinearCutItem>
        {
            new(
                Length: cutLength,
                Qty: liner.Qty * 2,
                BuildType: BuildItemType.Liner,
                CutType: CutItemType.Liner,
                GroupLabel: groupLabel
            )
        };
    }

    private string GetBuildListText(Liner liner)
    {
        string labelText = string.IsNullOrWhiteSpace(liner.Label)
            ? ""
            : $" - {liner.Label}";

        return $"{liner.Qty}x) {MathJC.RoundToSixteenth(liner.Width)}\" x {MathJC.RoundToSixteenth(liner.Height)}\" - {liner.Thickness.ToDisplayText()} Liner - {(int)liner.RollLength}\" Roll - {liner.PieceMode}{labelText}";
    }

    private string GetGroupLabel(Liner liner)
    {
        return $"{liner.Thickness.ToDisplayText()} Liner - {(int)liner.RollLength}\" Roll";
    }
}