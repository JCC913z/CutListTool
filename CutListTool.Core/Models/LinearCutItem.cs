namespace CutListTool.Core.Models;

public readonly record struct LinearCutItem(
    decimal Length,
    int Qty,
    CutItemType Type,
    string? GroupLabel = null
);
