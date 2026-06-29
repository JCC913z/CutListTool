using CutListTool.Core.Models;

namespace CutListTool.Core.Services;

public static class CutListGrouper
{
    public static List<LinearCutItem> GroupLinearCuts(List<LinearCutItem> rawCuts)
    {
        List<LinearCutItem> groupedCuts = rawCuts.GroupBy(cut => new
        {
            cut.BuildType,
            cut.CutType,
            cut.DisplayInSixteenths,
            cut.GroupLabel,
            cut.Length
        }).Select(group => new LinearCutItem(
            Length: group.Key.Length,
            Qty: group.Sum(cut => cut.Qty),
            BuildType: group.Key.BuildType,
            CutType: group.Key.CutType,
            DisplayInSixteenths: group.Key.DisplayInSixteenths,
            GroupLabel: group.Key.GroupLabel
        ))
        .OrderBy(cut => cut.BuildType)
        .ThenBy(cut => cut.CutType)
        .ThenBy(cut => cut.GroupLabel)
        .ThenByDescending(cut => cut.Length)
        .ToList();

        return groupedCuts;
    }

    public static List<CountCutItem> GroupCountCuts(List<CountCutItem> rawCuts)
    {
        List<CountCutItem> groupedCuts = rawCuts
            .GroupBy(cut => new
            {
                cut.BuildType,
                cut.CutType,
                cut.GroupLabel,
                cut.CountSize
            })
            .Select(group => new CountCutItem(
                CountSize: group.Key.CountSize,
                Qty: group.Sum(cut => cut.Qty),
                BuildType: group.Key.BuildType,
                CutType: group.Key.CutType,
                GroupLabel: group.Key.GroupLabel
            ))
            .OrderBy(cut => cut.BuildType)
            .ThenBy(cut => cut.CutType)
            .ThenBy(cut => cut.GroupLabel)
            .ThenByDescending(cut => cut.CountSize)
            .ToList();

        return groupedCuts;
    }
}