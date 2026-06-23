using System.Numerics;
using CutListTool.Core.Models;

namespace CutListTool.Core.Services;

public static class TurnVaneCalculator
{
    
    public static CalculatedTV Calculate(TurnVane TV, decimal DiagDeduction, decimal VaneSpacing)
    {
        int count = 0;
        decimal length = 0m;
        
        //formula
        decimal diag = (decimal)Vector2.Distance(new(0.0f, 0.0f), new((float)TV.CheekA, (float)TV.CheekB));
        diag -= DiagDeduction;
        
        
        return new CalculatedTV(
            Count: count,
            Length: length
        );        
    }
}
