using CutListTool.Core.Models;

namespace CutListTool.Core.Generators;

public class DuctmateGenerator
{
    private const decimal DMCutAllowance = 1.25m;

    public List<LinearCutItem> Generate(DuctmateFrame dmFrame)
    {
        return new List<LinearCutItem>
        {
            new(
                Length: dmFrame.Width - DMCutAllowance,
                Qty: dmFrame.Qty * 2,
                Type: CutItemType.Ductmate
            ),

            new(
                Length: dmFrame.Height - DMCutAllowance,
                Qty: dmFrame.Qty * 2,
                Type: CutItemType.Ductmate
            )
        };
    }
}
