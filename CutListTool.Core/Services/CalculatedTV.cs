namespace CutListTool.Core.Services;

public readonly record struct CalculatedTV(
    int Count,
    decimal Length,
    int QtyPerElbow
);
