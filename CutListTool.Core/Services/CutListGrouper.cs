using CutListTool.Core.Models;

namespace CutListTool.Core.Services;

public static class CutListGrouper
{
    public static List<LinearCutItem> GroupLinearCuts(List<LinearCutItem> rawCuts)
    {
        List<LinearCutItem> groupedCuts = rawCuts.GroupBy(cut => new
        {
            cut.Type,
            cut.GroupLabel,
            cut.Length
        }).Select(group => new LinearCutItem(
            Length: group.Key.Length,
            Qty: group.Sum(cut => cut.Qty),
            Type: group.Key.Type,
            GroupLabel: group.Key.GroupLabel
        )).OrderBy(cut => cut.Type).ThenBy(cut => cut.GroupLabel).ThenByDescending(cut => cut.Length).ToList();

        return groupedCuts;
    }
}