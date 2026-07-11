using CutListTool.Core.Models;
using CutListTool.Core.Models.OutsideDataHandlers;

namespace CutListTool.Core.Services.Outputs;

public static class CutListOutputBuilder
{
    public static CutListOutputData Build(
        List<BuildListLine> buildLines,
        List<LinearCutItem> groupedLinearCuts,
        List<CountCutItem> groupedCountCuts
    )
    {
        List<BuildItemType> buildTypes = buildLines
            .Select(line => line.BuildType)
            .Union(groupedLinearCuts.Select(cut => cut.BuildType))
            .Union(groupedCountCuts.Select(cut => cut.BuildType))
            .Distinct()
            .OrderBy(buildType => buildType)
            .ToList();

        List<CutListBuildSection> sections = [];

        foreach (BuildItemType buildType in buildTypes)
        {
            List<BuildListLine> matchingBuildLines = buildLines
                .Where(line => line.BuildType == buildType)
                .ToList();

            List<LinearCutItem> matchingLinearCuts = groupedLinearCuts
                .Where(cut => cut.BuildType == buildType)
                .ToList();

            List<CountCutItem> matchingCountCuts = groupedCountCuts
                .Where(cut => cut.BuildType == buildType)
                .ToList();

            List<CutListCutTypeSection> cutSections = BuildCutTypeSections(
                matchingLinearCuts,
                matchingCountCuts
            );

            sections.Add(new CutListBuildSection(
                BuildType: buildType,
                BuildLines: matchingBuildLines,
                CutSections: cutSections
            ));
        }

        return new CutListOutputData(
            Sections: sections
        );
    }

    private static List<CutListCutTypeSection> BuildCutTypeSections(
        List<LinearCutItem> linearCuts,
        List<CountCutItem> countCuts
    )
    {
        List<CutItemType> cutTypes = linearCuts
            .Select(cut => cut.CutType)
            .Union(countCuts.Select(cut => cut.CutType))
            .Distinct()
            .OrderBy(cutType => cutType)
            .ToList();

        List<CutListCutTypeSection> cutSections = [];

        foreach (CutItemType cutType in cutTypes)
        {
            List<LinearCutItem> linearCutsForCutType = linearCuts
                .Where(cut => cut.CutType == cutType)
                .ToList();

            List<CountCutItem> countCutsForCutType = countCuts
                .Where(cut => cut.CutType == cutType)
                .ToList();

            List<CutListGroupSection> groups = BuildGroupSections(
                linearCutsForCutType,
                countCutsForCutType
            );

            cutSections.Add(new CutListCutTypeSection(
                CutType: cutType,
                Groups: groups
            ));
        }

        return cutSections;
    }

    private static List<CutListGroupSection> BuildGroupSections(
        List<LinearCutItem> linearCuts,
        List<CountCutItem> countCuts
    )
    {
        List<string?> groupLabels = linearCuts
            .Select(cut => cut.GroupLabel)
            .Union(countCuts.Select(cut => cut.GroupLabel))
            .Distinct()
            .OrderBy(groupLabel => groupLabel)
            .ToList();

        List<CutListGroupSection> groups = [];

        foreach (string? groupLabel in groupLabels)
        {
            List<LinearCutItem> linearCutsInGroup = linearCuts
                .Where(cut => cut.GroupLabel == groupLabel)
                .ToList();

            List<CountCutItem> countCutsInGroup = countCuts
                .Where(cut => cut.GroupLabel == groupLabel)
                .ToList();

            groups.Add(new CutListGroupSection(
                GroupLabel: groupLabel,
                LinearCuts: linearCutsInGroup,
                CountCuts: countCutsInGroup
            ));
        }

        return groups;
    }
}