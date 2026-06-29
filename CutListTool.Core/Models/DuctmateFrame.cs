namespace CutListTool.Core.Models;

public readonly record struct DuctmateFrame(
    decimal Width,
    decimal Height,
    int Qty,
    string? Label = null
) : IBuildItem
{
    public BuildItemType BuildType => BuildItemType.Ductmate;
}
