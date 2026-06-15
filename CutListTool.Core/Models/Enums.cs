namespace CutListTool.Core.Models;

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
    Half_Inch,
    One_Inch,
    OneAndHalf_Inch    
}
public static class LinerThicknessExtensions
{
    public static string ToDisplayText(this LinerThickness thickness)
    {
        return thickness switch
        {
            LinerThickness.Half_Inch => "1/2\"",
            LinerThickness.One_Inch => "1\"",
            LinerThickness.OneAndHalf_Inch => "1-1/2\"",
            _ => thickness.ToString()
        };
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


