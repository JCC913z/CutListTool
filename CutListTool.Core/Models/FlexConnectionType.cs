namespace CutListTool.Core.Models;

public sealed record FlexConnectionType(
    string Key,
    string DisplayName,
    bool UsesFlangeOptions = false
);
