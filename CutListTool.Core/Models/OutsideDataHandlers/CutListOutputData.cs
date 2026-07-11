namespace CutListTool.Core.Models.OutsideDataHandlers;

public sealed record CutListOutputData(
    List<CutListBuildSection> Sections
);

public sealed record CutListBuildSection(
    BuildItemType BuildType,
    List<BuildListLine> BuildLines,
    List<CutListCutTypeSection> CutSections
);

public sealed record CutListCutTypeSection(
    CutItemType CutType,
    List<CutListGroupSection> Groups
);

public sealed record CutListGroupSection(
    string? GroupLabel,
    List<LinearCutItem> LinearCuts,
    List<CountCutItem> CountCuts
);