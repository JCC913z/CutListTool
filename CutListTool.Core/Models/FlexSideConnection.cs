namespace CutListTool.Core.Models;

public sealed record FlexSideConnection(
    FlexSide Side,
    string ConnectionTypeKey,
    FlangeDirection? FlangeDirection = null,
    decimal? FlangeSize = null
);
