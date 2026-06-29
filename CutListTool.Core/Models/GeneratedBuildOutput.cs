namespace CutListTool.Core.Models;

public sealed record GeneratedBuildOutput(
    BuildListLine BuildLine,
    List<LinearCutItem> LinearCuts,
    List<CountCutItem> CountCuts
);
