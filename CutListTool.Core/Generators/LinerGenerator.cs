using CutListTool.Core.Settings;
using CutListTool.Core.Models;

namespace CutListTool.Core.Generators;

public class LinerGenerator
{
    private readonly UserPreferences prefs;
    public LinerGenerator(UserPreferences preferences) {this.prefs = preferences;}

    public List<LinearCutItem> Generate(Liner liner)
    {
        string groupLabel = GetGroupLabel(liner);

        if(liner.PieceMode == LinerPieceMode.FourPiece)
        {
            return GenerateFourPieceCuts(liner, groupLabel);
        }

        return GenerateTwoPieceCuts(liner, groupLabel);

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
    private string GetGroupLabel(Liner liner)
    {
        return $"{liner.Thickness.ToDisplayText()} Liner - {(int)liner.RollLength}\" Roll";
    }
}