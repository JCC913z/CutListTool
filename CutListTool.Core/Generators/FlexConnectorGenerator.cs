using CutListTool.Core.Models;
using CutListTool.Core.Services;
using CutListTool.Core.Settings;

namespace CutListTool.Core.Generators;

public class FlexConnectorGenerator
{
    private readonly Dictionary<string, ConnectionType> connectionTypesByKey;
    private readonly UserPreferences prefs;

    public FlexConnectorGenerator(IEnumerable<ConnectionType> connectionTypes, UserPreferences preferences)
    {
        this.connectionTypesByKey = connectionTypes.ToDictionary(
            connectionType => connectionType.Key,
            StringComparer.OrdinalIgnoreCase
        );
        this.prefs = preferences;
    }

    public GeneratedBuildOutput Generate(FlexConnector flexConnector)
    {
        string groupLabel = GetGroupLabel(flexConnector);

        BuildListLine buildLine = new(
            BuildType: BuildItemType.Flex,
            Text: GetBuildListText(flexConnector)
        );

        List<LinearCutItem> linearCuts = [];
        
        switch (flexConnector.PieceCount)
        {
            case FlexPieceCount.OnePiece:
            {                    
                decimal length = 2 * (flexConnector.DimA + flexConnector.DimB) + (2 * prefs.CanvasAddPerSide);
                linearCuts.Add(
                    new LinearCutItem(
                        Length: length,
                        Qty: flexConnector.Qty,
                        BuildType: BuildItemType.Flex,
                        CutType: CutItemType.Flex,
                        GroupLabel: groupLabel
                    )
                );
                break;
            }

            case FlexPieceCount.TwoPiece:
            {                    
                decimal length = flexConnector.DimA + flexConnector.DimB + (2 * prefs.CanvasAddPerSide);
                linearCuts.Add(
                    new LinearCutItem(
                        Length: length,
                        Qty: flexConnector.Qty * 2,
                        BuildType: BuildItemType.Flex,
                        CutType: CutItemType.Flex,
                        GroupLabel: groupLabel
                    )
                );
                break;
            }

            case FlexPieceCount.FourPiece:
                linearCuts.AddRange(
                    new LinearCutItem(
                        Length: flexConnector.DimA + (2 * prefs.CanvasAddPerSide),
                        Qty: flexConnector.Qty * 2,
                        BuildType: BuildItemType.Flex,
                        CutType: CutItemType.Flex,
                        GroupLabel: groupLabel
                    ),

                    new LinearCutItem(
                        Length: flexConnector.DimB + (2 * prefs.CanvasAddPerSide),
                        Qty: flexConnector.Qty * 2,
                        BuildType: BuildItemType.Flex,
                        CutType: CutItemType.Flex,
                        GroupLabel: groupLabel
                    )
                );
                break;

            default:
            {
                throw new ArgumentOutOfRangeException(
                    nameof(flexConnector.PieceCount),
                    flexConnector.PieceCount,
                    "Unsupported flex piece count."
                );
            }
        };
        

        List<CountCutItem> countCuts = [];

        return new GeneratedBuildOutput(
            BuildLine: buildLine,
            LinearCuts: linearCuts,
            CountCuts: countCuts
        );
    }

    private string GetBuildListText(FlexConnector flexConnector)
    {
        string labelText = string.IsNullOrWhiteSpace(flexConnector.Label)
            ? ""
            : $"{flexConnector.Label} - ";

        string connectionAText = GetConnectionText(flexConnector.ConnectionA);
        string connectionBText = GetConnectionText(flexConnector.ConnectionB);

        string sideDetailsText =
            GetConnectionSideDetailsText("A", flexConnector.ConnectionA)
            + GetConnectionSideDetailsText("B", flexConnector.ConnectionB);

        return $"{labelText}({flexConnector.Qty}x) {MathJC.RoundToSixteenth(flexConnector.DimA)}\" x {MathJC.RoundToSixteenth(flexConnector.DimB)}\" - {connectionAText} -> {connectionBText}{sideDetailsText}";
    }

    private string GetGroupLabel(FlexConnector flexConnector)
    {
        string connectionAText = GetConnectionText(flexConnector.ConnectionA);
        string connectionBText = GetConnectionText(flexConnector.ConnectionB);

        string sideDetailsText =
            GetConnectionSideDetailsText("A", flexConnector.ConnectionA)
            + GetConnectionSideDetailsText("B", flexConnector.ConnectionB);

        return $"{connectionAText} -> {connectionBText}{sideDetailsText}";
    }

    private string GetConnectionText(Connection connection)
    {
        if (!connectionTypesByKey.TryGetValue(
            connection.ConnectionTypeKey,
            out ConnectionType? connectionType
        ))
        {
            return $"UNKNOWN CONNECTION TYPE: {connection.ConnectionTypeKey}";
        }

        if (connection.SideConnections is not null && connection.SideConnections.Count > 0)
        {
            return $"Custom {connectionType.DisplayName}";
        }

        if (connectionType.UsesFlangeOptions)
        {
            return GetFlangeText(
                connection.FlangeDirection,
                connection.FlangeSize
            );
        }

        return connectionType.DisplayName;
    }

    private string GetConnectionSideDetailsText(string connectionLabel, Connection connection)
    {
        List<PerSideConnection> sideConnections =
            connection.SideConnections ?? new List<PerSideConnection>();

        if (sideConnections.Count == 0)
        {
            return "";
        }

        List<string> sideTexts = sideConnections
            .OrderBy(sideConnection => sideConnection.Side)
            .Select(sideConnection => $"{sideConnection.Side}: {GetSideConnectionText(sideConnection)}")
            .ToList();

        return $" | {connectionLabel} sides: {string.Join(", ", sideTexts)}";
    }

    private string GetSideConnectionText(PerSideConnection sideConnection)
    {
        if (!connectionTypesByKey.TryGetValue(
            sideConnection.ConnectionTypeKey,
            out ConnectionType? connectionType
        ))
        {
            return $"UNKNOWN CONNECTION TYPE: {sideConnection.ConnectionTypeKey}";
        }

        if (connectionType.UsesFlangeOptions)
        {
            return GetFlangeText(
                sideConnection.FlangeDirection,
                sideConnection.FlangeSize
            );
        }

        return connectionType.DisplayName;
    }

    private string GetFlangeText(
        FlangeDirection? flangeDirection,
        decimal? flangeSize
    )
    {
        if (flangeDirection is null)
        {
            return "MISSING FLANGE DIRECTION";
        }

        if (flangeDirection == FlangeDirection.Straight)
        {
            return "Straight";
        }

        if (flangeSize is null)
        {
            return $"F{flangeDirection.Value.ToString()[0]} - MISSING FLANGE SIZE";
        }

        string flangeSizeText = flangeSize.Value.ToString("0.###");

        return $"{flangeSizeText}\" F{flangeDirection.Value.ToString()[0]}";
    }
}