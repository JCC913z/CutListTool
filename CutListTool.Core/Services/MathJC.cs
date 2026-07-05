using System.Net.NetworkInformation;

namespace CutListTool.Core.Services;

public static class MathJC
{
    public const decimal pi = 3.14159265358979m;

    public static string RoundToSixteenth(decimal number)
    {
        int i = (int)number;
        int d = 16;
        decimal r = number - i;

        int n = (int)Math.Round(r*16);
        if (n == 16) { i++; n = 0; }        

        while (n > 0 && n % 2 == 0)
        {
            n /= 2;
            d /= 2;
        }

        string result = i.ToString();
        if(n > 0) { result += $"-{n}/{d}"; }
        
        return result;
    }

    public static T[] ReverseArray<T>(T[] array)
    {
        T[] revArray = new T[array.Length];

        int length = array.Length;
        for (int i = 0; i < length; i++)
        {            revArray[i] = array[length - (i + 1)];
        }

        return revArray;
    }

    public static decimal RoundStretchOut(decimal nominalDiameter, bool se = false)
    {
        decimal diameter = se ? nominalDiameter - 0.125m : nominalDiameter;
        decimal stretchOut = diameter * pi;

        return stretchOut;
    }
}
