using System.Numerics;
using CutListTool.Core.Models;

namespace CutListTool.Core.Services;

public static class TurnVaneCalculator
{    
    public static CalculatedTV Calculate(TurnVane TV, decimal DiagDeduction, decimal VaneSpacing, decimal SplitterGap)
    {
        int count;
        decimal length;
        int qtyPerElbow;
        
        decimal diag = (decimal)Math.Sqrt((double)(TV.CheekA*TV.CheekA + TV.CheekB*TV.CheekB));

        diag -= DiagDeduction + 2 * TV.Liner.ToDecimalThickness();
        count = (int)decimal.Floor(diag / VaneSpacing) + 1;

        length = TV.Heel - 0.25m - (TV.Liner.ToDecimalThickness() / 2m);
        
        length -= (TV.SplitVanes - 1) * SplitterGap;

        length /= TV.SplitVanes;
        
        qtyPerElbow = TV.SplitVanes;

        return new CalculatedTV(
            Count: count,
            Length: length,
            QtyPerElbow: qtyPerElbow
        );        
    }
}
