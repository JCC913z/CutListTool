namespace CutListTool.Core.Settings;

public sealed record UserPreferences
{
    //Ductmate
    public decimal DMCutAllowance {get; init;} = 1.25m;

    //Liner
    public decimal FourPieceWidthDeduction {get; init;} = 2m;
    public decimal FourPieceHeightDeduction {get; init;} = 0.5m;
    public decimal TwoPieceDeduction {get; init;} = 1.5m;

    //TurnVane
    public decimal DiagonalDeduction {get; init;} = 2m;
    public decimal VaneSpacing {get; init;} = 4.75m;

    //Flex

    //FR Channel
}