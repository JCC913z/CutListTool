namespace CutListTool.Core.Models;

public sealed record CutListRequest(
    List<CutListPackageRequest> Packages
);