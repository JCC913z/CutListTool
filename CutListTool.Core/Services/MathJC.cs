namespace CutListTool.Core.Services;

public static class MathJC
{
    public static string RoundToSixteenth(decimal number)
    {
        int i = (int)number;
        int d = 16;
        decimal r = number - i;

        int n = (int)Math.Round(r*16);

        while (n > 0 && n % 2 == 0)
        {
            n /= 2;
            d /= 2;
        }

        string result = $"{i}";
        if(n > 0) { result += $"-{n}/{d}"; }

        return result;
    }

}
