namespace CutListTool.Core.Models;

public sealed record PerSideConnection(
    FittingSide Side,
    string ConnectionTypeKey,
    FlangeDirection? FlangeDirection = null,
    decimal? FlangeSize = null
);
