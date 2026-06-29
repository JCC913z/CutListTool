namespace CutListTool.Core.Models;

public readonly record struct CountCutItem(
    int CountSize,
    int Qty,
    BuildItemType BuildType,
    CutItemType CutType,
    string? GroupLabel = null
);
