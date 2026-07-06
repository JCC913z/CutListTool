using CutListTool.Core.Models;
using CutListTool.Core.Services;
using CutListTool.Core.Settings;

namespace CutListTool.Core.Generators;

public class FlexConnectorGenerator
{
    private readonly Dictionary<string, ConnectionType> connectionTypesByKey;
    private readonly UserPreferences prefs;

    private readonly record struct FlexCutPiece(
        decimal Length,
        int QtyPerFlex
    );

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
        string cutGroupLabel = $"{flexConnector.Size} Flex";

        BuildListLine buildLine = new(
            BuildType: BuildItemType.Flex,
            Text: GetBuildListText(flexConnector)
        );

        List<FlexCutPiece> flexCutPieces = GetFlexCutPieces(flexConnector);

        List<LinearCutItem> linearCuts = flexCutPieces
            .Select(piece => new LinearCutItem(
                Length: piece.Length,
                Qty: flexConnector.Qty * piece.QtyPerFlex,
                BuildType: BuildItemType.Flex,
                CutType: CutItemType.Flex,
                GroupLabel: cutGroupLabel
            ))
            .ToList();

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

        string layoutText = GetLayoutText(flexConnector, prefs.ReverseLayoutOrder);

        string buildListCutText = GetBuildListCutText(flexConnector);

        string dimensionText = GetDimensionText(flexConnector);

        return $"{labelText}{flexConnector.Qty}x) {dimensionText} - {{{buildListCutText} {flexSizeText}}}"
            + Environment.NewLine
            + $"\tDetails: {connectionAText} -> {connectionBText}{sideDetailsText}"
            + Environment.NewLine
            + $"\tLayout: {layoutText}";
    }

    private string GetDimensionText(FlexConnector flexConnector)
    {
        return flexConnector.Shape switch
        {
            ConnectorShape.Rectangular =>
                $"{MathJC.RoundToSixteenth(flexConnector.DimA)}\" x {MathJC.RoundToSixteenth(flexConnector.DimB)}\"",

            ConnectorShape.Round when flexConnector.DimA == flexConnector.DimB =>
                $"{MathJC.RoundToSixteenth(flexConnector.DimA)}\" Round",

            ConnectorShape.Round =>
                $"{MathJC.RoundToSixteenth(flexConnector.DimA)}\" Round -> {MathJC.RoundToSixteenth(flexConnector.DimB)}\" Round",

            _ => throw new ArgumentOutOfRangeException(
                nameof(flexConnector.Shape),
                flexConnector.Shape,
                "Unsupported connector shape."
            )
        };
    }

   private string GetLayoutText(FlexConnector flexConnector, bool reversed = false)
    {
        return flexConnector.Shape switch
        {
            ConnectorShape.Rectangular => GetRectangularLayoutText(flexConnector, reversed),
            ConnectorShape.Round => GetRoundLayoutText(flexConnector, reversed),

            _ => throw new ArgumentOutOfRangeException(
                nameof(flexConnector.Shape),
                flexConnector.Shape,
                "Unsupported connector shape."
            )
        };

            
    }

    private string GetRectangularLayoutText(FlexConnector flexConnector, bool reversed = false)
    {
            int n = flexConnector.PieceCount switch
        {
            FlexPieceCount.OnePiece => 5,
            FlexPieceCount.TwoPiece => 3,
            FlexPieceCount.FourPiece => 2,  
            _ => throw new ArgumentOutOfRangeException(nameof(flexConnector.PieceCount), flexConnector.PieceCount, "Unsupported flex piece count.")
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
                $"& [{MathJC.RoundToSixteenth(prefs.CanvasAddPerSide)}\" - " +
                $"{MathJC.RoundToSixteenth(flexConnector.DimB + prefs.CanvasAddPerSide)}\"]";
        }
    }
    private string GetRoundLayoutText(FlexConnector flexConnector, bool reversed = false)
    {
        decimal cutLength = GetRoundFlexCutPieces(flexConnector)[0].Length;

        string layoutText = $"[{MathJC.RoundToSixteenth(prefs.CanvasAddPerSide)}\" - " +
            $"{MathJC.RoundToSixteenth(cutLength - prefs.CanvasAddPerSide)}\"]";

        return layoutText;
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

    private List<FlexCutPiece> GetFlexCutPieces(FlexConnector flexConnector)
    {
        return flexConnector.Shape switch
        {
            ConnectorShape.Rectangular => GetRectangularFlexCutPieces(flexConnector),
            ConnectorShape.Round => GetRoundFlexCutPieces(flexConnector),

            _ => throw new ArgumentOutOfRangeException(
                nameof(flexConnector.Shape),
                flexConnector.Shape,
                "Unsupported connector shape."
            )
        };        
    }

    private List<FlexCutPiece> GetRectangularFlexCutPieces(FlexConnector flexConnector)
    {
        return flexConnector.PieceCount switch
        {
            FlexPieceCount.OnePiece =>
            [
                new FlexCutPiece(
                    Length: 2 * (flexConnector.DimA + flexConnector.DimB) + (2 * prefs.CanvasAddPerSide),
                    QtyPerFlex: 1
                )
            ],

            FlexPieceCount.TwoPiece =>
            [
                new FlexCutPiece(
                    Length: flexConnector.DimA + flexConnector.DimB + (2 * prefs.CanvasAddPerSide),
                    QtyPerFlex: 2
                )
            ],

            FlexPieceCount.FourPiece =>
            [
                new FlexCutPiece(
                    Length: flexConnector.DimA + (2 * prefs.CanvasAddPerSide),
                    QtyPerFlex: 2
                ),

                new FlexCutPiece(
                    Length: flexConnector.DimB + (2 * prefs.CanvasAddPerSide),
                    QtyPerFlex: 2
                )
            ],

            _ => throw new ArgumentOutOfRangeException(
                nameof(flexConnector.PieceCount),
                flexConnector.PieceCount,
                "Unsupported flex piece count."
            )
        };
    }

    private List<FlexCutPiece> GetRoundFlexCutPieces(FlexConnector flexConnector)
    {        
        decimal length = 0m;
        decimal diameter = (flexConnector.DimB > flexConnector.DimA) ? flexConnector.DimB : flexConnector.DimA;
        bool smallEnd = flexConnector.ConnectionA.SmallEnd && flexConnector.ConnectionB.SmallEnd;

        length = MathJC.RoundStretchOut(diameter, smallEnd);
        length += 2 * prefs.CanvasAddPerSide;

        return
        [
            new FlexCutPiece(
                Length: length,
                QtyPerFlex: 1
            )
        ];
    }

    private string GetBuildListCutText(FlexConnector flexConnector)
    {
        List<FlexCutPiece> flexCutPieces = GetFlexCutPieces(flexConnector);

        List<string> cutTexts = flexCutPieces
            .Select(piece =>
            {
                string lengthText = $"{MathJC.RoundToSixteenth(piece.Length)}\"";

                if (piece.QtyPerFlex == 1)
                {
                    return lengthText;
                }

                return $"{lengthText} [x{piece.QtyPerFlex}]";
            })
            .ToList();

        return string.Join(" & ", cutTexts);
    }

}