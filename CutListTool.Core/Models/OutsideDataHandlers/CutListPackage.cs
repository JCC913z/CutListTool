using CutListTool.Core.Models.OutsideDataHandlers;

namespace CutListTool.Core.Models.OutsideDataHandlers;

public sealed record CutListPackage(
    string Name,
    List<BuildItemType> IncludedBuildTypes,
    CutListOutputFormat OutputFormat,
    CutListOutputData OutputData,
    string? RenderedOutput = null
);