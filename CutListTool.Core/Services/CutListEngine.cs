using CutListTool.Core.Generators;
using CutListTool.Core.Models;
using CutListTool.Core.Models.OutsideDataHandlers;
using CutListTool.Core.Services.Outputs;
using CutListTool.Core.Settings;

namespace CutListTool.Core.Services;

public static class CutListEngine
{
    public static CutListResult Generate(
        CutListInputData input,
        CutListRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(request);

        List<GeneratedBuildOutput> generatedOutputs = GenerateRawOutputs(input);

        List<CutListPackage> packages = request.Packages
            .Select(packageRequest => BuildPackage(packageRequest, generatedOutputs))
            .ToList();

        return new CutListResult(
            Packages: packages
        );
    }

    private static List<GeneratedBuildOutput> GenerateRawOutputs(CutListInputData input)
    {
        UserPreferences prefs = input.Preferences;

        DuctmateGenerator ductmateGenerator = new(prefs);
        LinerGenerator linerGenerator = new(prefs);
        TurnVaneGenerator turnVaneGenerator = new(prefs);
        FlexConnectorGenerator flexConnectorGenerator = new(input.ConnectionTypes, prefs);

        List<GeneratedBuildOutput> generatedOutputs = new();

        foreach (DuctmateFrame ductmateFrame in input.DuctmateFrames)
        {
            generatedOutputs.Add(ductmateGenerator.Generate(ductmateFrame));
        }

        foreach (Liner liner in input.Liners)
        {
            generatedOutputs.Add(linerGenerator.Generate(liner));
        }

        foreach (TurnVane turnVane in input.TurnVanes)
        {
            generatedOutputs.Add(turnVaneGenerator.Generate(turnVane));
        }

        foreach (FlexConnector flexConnector in input.FlexConnectors)
        {
            generatedOutputs.Add(flexConnectorGenerator.Generate(flexConnector));
        }

        return generatedOutputs;
    }

    private static CutListPackage BuildPackage(
        CutListPackageRequest packageRequest,
        List<GeneratedBuildOutput> generatedOutputs
    )
    {
        List<BuildItemType> includedBuildTypes = packageRequest.IncludedBuildTypes
            .Distinct()
            .OrderBy(buildType => buildType)
            .ToList();

        HashSet<BuildItemType> includedBuildTypeSet = includedBuildTypes.ToHashSet();

        List<GeneratedBuildOutput> selectedOutputs = generatedOutputs
            .Where(output => includedBuildTypeSet.Contains(output.BuildLine.BuildType))
            .ToList();

        List<BuildListLine> buildLines = selectedOutputs
            .Select(output => output.BuildLine)
            .ToList();

        List<LinearCutItem> rawLinearCuts = selectedOutputs
            .SelectMany(output => output.LinearCuts)
            .Where(cut => includedBuildTypeSet.Contains(cut.BuildType))
            .ToList();

        List<CountCutItem> rawCountCuts = selectedOutputs
            .SelectMany(output => output.CountCuts)
            .Where(cut => includedBuildTypeSet.Contains(cut.BuildType))
            .ToList();

        List<LinearCutItem> groupedLinearCuts = CutListGrouper.GroupLinearCuts(rawLinearCuts);
        List<CountCutItem> groupedCountCuts = CutListGrouper.GroupCountCuts(rawCountCuts);

        CutListOutputData outputData = CutListOutputBuilder.Build(
            buildLines,
            groupedLinearCuts,
            groupedCountCuts
        );

        string? renderedOutput = RenderOutput(
            outputData,
            packageRequest.OutputFormat
        );

        return new CutListPackage(
            Name: packageRequest.Name,
            IncludedBuildTypes: includedBuildTypes,
            OutputFormat: packageRequest.OutputFormat,
            OutputData: outputData,
            RenderedOutput: renderedOutput,
            OutputFileName: packageRequest.OutputFileName
        );
    }

    private static string? RenderOutput(
        CutListOutputData outputData,
        CutListOutputFormat outputFormat
    )
    {
        return outputFormat switch
        {
            CutListOutputFormat.Structured => null,
            CutListOutputFormat.Text => new TextCutListOutputService().Generate(outputData),
            CutListOutputFormat.Json => new JsonCutListOutputService().Generate(outputData),
            _ => throw new ArgumentOutOfRangeException(nameof(outputFormat), outputFormat, null)
        };
    }
}