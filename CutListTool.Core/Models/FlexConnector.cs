namespace CutListTool.Core.Models;

public readonly record struct FlexConnector(
    decimal DimA,
    decimal DimB,
    int Qty,
    string DefaultConnectionTypeKey,
    List<FlexSideConnection>? SideConnections = null,
    string? Label = null
) : IBuildItem
{
    public BuildItemType BuildType => BuildItemType.Flex;
}
