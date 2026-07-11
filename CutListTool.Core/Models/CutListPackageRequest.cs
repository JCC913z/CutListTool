namespace CutListTool.Core.Models;

public sealed record CutListPackageRequest(
    string Name,
    List<BuildItemType> IncludedBuildTypes,
    CutListOutputFormat OutputFormat = CutListOutputFormat.Structured
);