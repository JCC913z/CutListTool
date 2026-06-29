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
    public BuildItemType BuildType => BuildItemType.Liner;
}