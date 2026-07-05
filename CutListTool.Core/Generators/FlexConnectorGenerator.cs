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
        string cutGroupLabel = $"{flexConnector.Size.ToString()} Flex";

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
                        GroupLabel: cutGroupLabel
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
                        GroupLabel: cutGroupLabel
                    )
                );
                break;
            }

            case FlexPieceCount.FourPiece:
                linearCuts.AddRange([
                    new LinearCutItem(
                        Length: flexConnector.DimA + (2 * prefs.CanvasAddPerSide),
                        Qty: flexConnector.Qty * 2,
                        BuildType: BuildItemType.Flex,
                        CutType: CutItemType.Flex,
                        GroupLabel: cutGroupLabel
                    ),

                    new LinearCutItem(
                        Length: flexConnector.DimB + (2 * prefs.CanvasAddPerSide),
                        Qty: flexConnector.Qty * 2,
                        BuildType: BuildItemType.Flex,
                        CutType: CutItemType.Flex,
                        GroupLabel: cutGroupLabel
                    )
                ]);
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

        string flexSizeText = $"{flexConnector.Size.ToString()} Flex";

        string connectionAText = GetConnectionText(flexConnector.ConnectionA);
        string connectionBText = GetConnectionText(flexConnector.ConnectionB);

        string sideDetailsText =
            GetConnectionSideDetailsText("A", flexConnector.ConnectionA)
            + GetConnectionSideDetailsText("B", flexConnector.ConnectionB);

        string layoutText = GetLayoutText(flexConnector);

        return $"{labelText}{flexConnector.Qty}x) {MathJC.RoundToSixteenth(flexConnector.DimA)}\" x {MathJC.RoundToSixteenth(flexConnector.DimB)}\" - {flexSizeText}"
            + Environment.NewLine
            + $"\tDetails: {connectionAText} -> {connectionBText}{sideDetailsText}"
            + Environment.NewLine
            + $"\tLayout: {layoutText}";
    }

    private string GetLayoutText(FlexConnector flexConnector, bool reversed = false)
    {
        int n = flexConnector.PieceCount switch
        {
          FlexPieceCount.OnePiece => 5,
          FlexPieceCount.TwoPiece => 3,
          FlexPieceCount.FourPiece => 2,  
          _ => 0
        };
        decimal[] layoutLengths = new decimal[n];

        decimal largerSide;
        decimal smallerSide;

        if (flexConnector.DimB > flexConnector.DimA) { largerSide = flexConnector.DimB; smallerSide = flexConnector.DimA; }
        else { largerSide = flexConnector.DimA; smallerSide = flexConnector.DimB; }

        layoutLengths[0] = prefs.CanvasAddPerSide;

        if (n > 2) //1 & 2 Piece
        {
            for (int i = 1; i < n; i++)
            {
                layoutLengths[i] = layoutLengths[i-1] + ( (i%2 != 0) ? smallerSide : largerSide );
            }   

            if (reversed)
            {
                layoutLengths = MathJC.ReverseArray(layoutLengths);
            }

            string layoutText = "[";
            int j = 0;
            while(j < layoutLengths.Length)
            {
                if(j > 0) {layoutText += " - ";}
                layoutText += $"{MathJC.RoundToSixteenth(layoutLengths[j])}\"";
                j++;                
            }
            layoutText += "]";

            return layoutText;
        }
        else //4 Piece
        {
            return $"[{MathJC.RoundToSixteenth(prefs.CanvasAddPerSide)}\" - " +
                $"{MathJC.RoundToSixteenth(flexConnector.DimA + prefs.CanvasAddPerSide)}\"] " +
                $"And [{MathJC.RoundToSixteenth(prefs.CanvasAddPerSide)}\" - " +
                $"{MathJC.RoundToSixteenth(flexConnector.DimB + prefs.CanvasAddPerSide)}\"]";
        }        
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
            connection.SideConnections ?? [];

        if (sideConnections.Count == 0)
        {
            return "";
        }

        List<string> groupedSideTexts = sideConnections
            .GroupBy(sideConnection => GetSideConnectionText(sideConnection))
            .OrderBy(group => group.Min(sideConnection => sideConnection.Side))
            .Select(group =>
            {
                List<string> sideNames = group
                    .OrderBy(sideConnection => sideConnection.Side)
                    .Select(sideConnection => sideConnection.Side.ToString())
                    .ToList();

                return $"{JoinSideNames(sideNames)}: {group.Key}";
            })
            .ToList();

        return $" | {connectionLabel} sides: {string.Join(", ", groupedSideTexts)}";
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

    private string JoinSideNames(List<string> sideNames)
    {
        if (sideNames.Count == 0)
        {
            return "";
        }

        if (sideNames.Count == 1)
        {
            return sideNames[0];
        }

        if (sideNames.Count == 2)
        {
            return $"{sideNames[0]} and {sideNames[1]}";
        }

        return $"{string.Join(", ", sideNames.Take(sideNames.Count - 1))}, and {sideNames.Last()}";
    }
}