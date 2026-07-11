namespace CutListTool.Core.Models.OutsideDataHandlers;

public sealed record CutListRequest(
    List<CutListPackageRequest> Packages
);