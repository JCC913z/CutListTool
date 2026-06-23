using CutListTool.Core.Models;
using CutListTool.Core.Settings;

namespace CutListTool.Core.Generators;

public class DuctmateGenerator
{
    private readonly UserPreferences prefs;
    public DuctmateGenerator(UserPreferences preferences) {this.prefs = preferences;}

    public List<LinearCutItem> Generate(DuctmateFrame dmFrame)
    {
        return new List<LinearCutItem>
        {
            new(
                Length: dmFrame.Width - prefs.DMCutAllowance,
                Qty: dmFrame.Qty * 2,
                Type: CutItemType.Ductmate
            ),

            new(
                Length: dmFrame.Height - prefs.DMCutAllowance,
                Qty: dmFrame.Qty * 2,
                Type: CutItemType.Ductmate
            )
        };
    }
}
