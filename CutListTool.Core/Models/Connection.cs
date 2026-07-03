namespace CutListTool.Core.Models;

public sealed record FlexConnection(
    string ConnectionTypeKey,
    FlangeDirection? FlangeDirection = null,
    decimal? FlangeSize = null,
    List<FlexSideConnection>? SideConnections = null
);
