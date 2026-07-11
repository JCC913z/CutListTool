namespace CutListTool.Core.Models.OutsideDataHandlers;

public sealed record CutListPackageRequest(
    string Name,
    List<BuildItemType> IncludedBuildTypes,
    CutListOutputFormat OutputFormat = CutListOutputFormat.Structured
);