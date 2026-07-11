using CutListTool.Core.Settings;

namespace CutListTool.Core.Models.OutsideDataHandlers;

public sealed record CutListInputData
{
    public List<DuctmateFrame> DuctmateFrames { get; init; } = new();
    public List<Liner> Liners { get; init; } = new();
    public List<TurnVane> TurnVanes { get; init; } = new();

    public List<ConnectionType> ConnectionTypes { get; init; } = new();
    public List<FlexConnector> FlexConnectors { get; init; } = new();

    public UserPreferences Preferences { get; init; } = new();
}