namespace CutListTool.Core.Models;

public readonly record struct FlexConnector(
    decimal DimA,
    decimal DimB,
    int Qty,
    FlexConnection ConnectionA,
    FlexConnection ConnectionB,
    string? Label = null
) : IBuildItem
{
    public BuildItemType BuildType => BuildItemType.Flex;
}
