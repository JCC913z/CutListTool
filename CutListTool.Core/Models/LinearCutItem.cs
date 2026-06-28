namespace CutListTool.Core.Models;

public readonly record struct LinearCutItem(
    decimal Length,
    int Qty,
    BuildItemType BuildType,
    CutItemType CutType,
    string? GroupLabel = null
);
