namespace CutListTool.Core.Models;

public readonly record struct Liner(
    decimal Width,
    decimal Height,
    int Qty,
    LinerRollLength RollLength,
    LinerThickness Thickness,
    LinerPieceMode PieceMode,
    string? Label = null
) : IBuildItem
{
    public BuildItemType Type => BuildItemType.Liner;

    public string GetBuildListText()
    {
        string labelText = string.IsNullOrWhiteSpace(Label) ? "" : $" - {Label}";

        return $"{Qty}x) {Width}\" x {Height}\" - {Thickness.ToDisplayText()} Liner - {(int)RollLength}\" Roll - {PieceMode}{labelText}";
    }

}