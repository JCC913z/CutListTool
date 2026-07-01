namespace CutListTool.Core.Models;

public enum BuildItemType
{
    Ductmate,
    Liner,
    TurnVane,
    Flex,
    FilterRackChannel
}

public enum CutItemType
{
    Ductmate,
    Liner,
    TV_Vanes,
    TV_Rails,
    Flex,
    FR_Channel
}

public enum LinerThickness
{
    None,
    Half_Inch,
    One_Inch,
    OneAndHalf_Inch    
}
public static class LinerThicknessExtensions
{
    public static string ToDisplayText(this LinerThickness thickness)
    {
        return thickness switch
        {   LinerThickness.None => "No",
            LinerThickness.Half_Inch => "1/2\"",
            LinerThickness.One_Inch => "1\"",
            LinerThickness.OneAndHalf_Inch => "1-1/2\"",
            _ => thickness.ToString()
        };
    }

        public static decimal ToDecimalThickness(this LinerThickness thickness)
    {
        return thickness switch
        {
            LinerThickness.None => 0m,
            LinerThickness.Half_Inch => 0.5m,
            LinerThickness.One_Inch => 1m,
            LinerThickness.OneAndHalf_Inch => 1.5m,
            _ => throw new ArgumentOutOfRangeException(nameof(thickness), thickness, null)
        };
    }

    public static bool HasLiner(this LinerThickness thickness)
    {
        return thickness != LinerThickness.None;
    }
}

public enum LinerRollLength
{
    Roll56 = 56,
    Roll59 = 59
}

public enum LinerPieceMode
{
    TwoPiece,
    FourPiece
}

public enum FlexSize
{
    Large,
    Small
}

public enum FlexSide
{
    Top,
    Bottom,
    Left,
    Right
}

public enum FlangeDirection
{
    In,
    Out
}
