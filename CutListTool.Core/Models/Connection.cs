namespace CutListTool.Core.Models;

public sealed record Connection(
    string ConnectionTypeKey,
    FlangeDirection? FlangeDirection = null,
    decimal? FlangeSize = null,
    List<PerSideConnection>? SideConnections = null
);
