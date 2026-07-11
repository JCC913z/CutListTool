using System.Text;
using CutListTool.Core.Models;
using CutListTool.Core.Models.OutsideDataHandlers;

namespace CutListTool.Core.Services.Outputs;

public sealed class TextCutListOutputService
{
    public string Generate(CutListOutputData outputData)
    {
        StringBuilder output = new();

        foreach (CutListBuildSection buildSection in outputData.Sections)
        {
            output.AppendLine("\t" + BuildTypeBorder(buildSection.BuildType));
            output.AppendLine("\t" + $"||-->  {buildSection.BuildType.ToString().ToUpper()}  <--||");
            output.AppendLine("\t" + BuildTypeBorder(buildSection.BuildType));
            output.AppendLine();

            output.AppendLine("*------------*");
            output.AppendLine("| BUILD LIST |");
            output.AppendLine("*------------*");

            foreach (BuildListLine buildLine in buildSection.BuildLines)
            {
                output.AppendLine(buildLine.Text);

                if (buildSection.BuildType == BuildItemType.Flex)
                {
                    output.AppendLine();
                }
            }

            output.AppendLine();
            output.AppendLine("*----------*");
            output.AppendLine("| CUT LIST |");
            output.AppendLine("*----------*");

            bool showCutTypeHeaders = buildSection.CutSections.Count > 1;

            foreach (CutListCutTypeSection cutSection in buildSection.CutSections)
            {
                if (showCutTypeHeaders)
                {
                    output.AppendLine();
                    output.AppendLine(cutSection.CutType.ToString());
                    output.AppendLine("----------");
                }

                foreach (CutListGroupSection groupSection in cutSection.Groups)
                {
                    if (!string.IsNullOrWhiteSpace(groupSection.GroupLabel))
                    {
                        output.AppendLine(groupSection.GroupLabel);
                        output.AppendLine("----------");
                    }

                    foreach (LinearCutItem cutItem in groupSection.LinearCuts)
                    {
                        string displayLength = cutItem.Length.ToString();

                        if (cutItem.DisplayInSixteenths)
                        {
                            displayLength = MathJC.RoundToSixteenth(cutItem.Length);
                        }

                        output.AppendLine($"{cutItem.Qty} @ {displayLength}\"");
                    }

                    foreach (CountCutItem cutItem in groupSection.CountCuts)
                    {
                        if (cutItem.CutType == CutItemType.TV_Rails)
                        {
                            output.AppendLine($"{cutItem.Qty} @ {cutItem.CountSize}-vane rail");
                        }
                        else
                        {
                            output.AppendLine($"{cutItem.Qty} @ {cutItem.CountSize}");
                        }
                    }

                    output.AppendLine();
                }
            }

            output.AppendLine();
        }

        return output.ToString();
    }

    private static string BuildTypeBorder(BuildItemType buildType)
    {
        string border = "*";

        for (int i = 0; i < buildType.ToString().Length + 12; i++)
        {
            border += "=";
        }

        border += "*";

        return border;
    }
}