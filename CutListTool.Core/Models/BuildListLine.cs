namespace CutListTool.Core.Models;

public readonly record struct BuildListLine(
    BuildItemType BuildType,
    string Text
);