using CutListTool.Core.Models.Outputs;

namespace CutListTool.Core.Models;

public sealed record CutListPackage(
    string Name,
    List<BuildItemType> IncludedBuildTypes,
    CutListOutputFormat OutputFormat,
    CutListOutputData OutputData,
    string? RenderedOutput = null
);