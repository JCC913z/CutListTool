namespace CutListTool.Core.Models;

public readonly record struct FlexConnector(
    decimal DimA,
    decimal DimB,
    int Qty,
    Connection ConnectionA,
    Connection ConnectionB,
    string? Label = null
) : IBuildItem
{
    public BuildItemType BuildType => BuildItemType.Flex;
}
