namespace CutListTool.Core.Models;

public readonly record struct FlexConnector(
    decimal DimA,
    decimal DimB,
    int Qty,
    FlexSize Size,
    FlexPieceCount PieceCount,
    Connection ConnectionA,
    Connection ConnectionB,
    string? Label = null,
    ConnectorShape Shape = ConnectorShape.Rectangular
) : IBuildItem
{
    public BuildItemType BuildType => BuildItemType.Flex;
}
