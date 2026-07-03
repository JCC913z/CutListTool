namespace CutListTool.Core.Models;

public sealed record ConnectionType(
    string Key,
    string DisplayName,
    bool UsesFlangeOptions = false
);
