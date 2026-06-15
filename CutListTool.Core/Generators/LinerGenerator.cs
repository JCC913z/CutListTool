using CutListTool.Core.Models;

namespace CutListTool.Core.Generators;

public class LinerGenerator
{
    private const decimal FourPieceWidthDeduction = 2m;
    private const decimal FourPieceHeightDeduction = 0.5m;
    private const decimal TwoPieceDeduction = 1.5m;

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
                Length: liner.Width - FourPieceWidthDeduction,
                Qty: liner.Qty * 2,
                Type: CutItemType.Liner,
                GroupLabel: groupLabel
            ),

            new(
                Length: liner.Height - FourPieceHeightDeduction,
                Qty: liner.Qty * 2,
                Type: CutItemType.Liner,
                GroupLabel: groupLabel
            )
        };
    }

    private List<LinearCutItem> GenerateTwoPieceCuts(Liner liner, string groupLabel)
    {
        decimal cutLength = liner.Width + liner.Height - TwoPieceDeduction;

        return new List<LinearCutItem>
        {
            new(
                Length: cutLength,
                Qty: liner.Qty * 2,
                Type: CutItemType.Liner,
                GroupLabel: groupLabel
            )
        };
    }
    private string GetGroupLabel(Liner liner)
    {
        return $"{liner.Thickness.ToDisplayText()} Liner - {(int)liner.RollLength}\" Roll";
    }
}