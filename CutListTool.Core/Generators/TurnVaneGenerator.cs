using CutListTool.Core.Models;
using CutListTool.Core.Services;
using CutListTool.Core.Settings;

namespace CutListTool.Core.Generators;

public class TurnVaneGenerator
{
    private readonly UserPreferences prefs;

    public TurnVaneGenerator(UserPreferences preferences)
    {
        this.prefs = preferences;
    }

    public GeneratedBuildOutput Generate(
        TurnVane turnVane,
        decimal? diagonalDeduction = null,
        decimal? vaneSpacing = null,
        decimal? splitterGap = null
    )
    {
        CalculatedTV calculated = TurnVaneCalculator.Calculate(
            TV: turnVane,
            DiagDeduction: diagonalDeduction ?? prefs.DiagonalDeduction,
            VaneSpacing: vaneSpacing ?? prefs.VaneSpacing,
            SplitterGap: splitterGap ?? prefs.SplitterGap
        );

        BuildListLine buildLine = new(
            BuildType: BuildItemType.TurnVane,
            Text: GetBuildListText(turnVane, calculated)
        );

        List<LinearCutItem> linearCuts = new()
        {
            new(
                Length: calculated.Length,
                Qty: turnVane.Qty * calculated.Count * calculated.QtyPerElbow,
                BuildType: BuildItemType.TurnVane,
                CutType: CutItemType.TV_Vanes,
                DisplayInSixteenths: false
            )
        };

        List<CountCutItem> countCuts = new()
        {
            new(
                CountSize: calculated.Count,
                Qty: turnVane.Qty * 2 * turnVane.SplitVanes,
                BuildType: BuildItemType.TurnVane,
                CutType: CutItemType.TV_Rails
            )
        };

        return new GeneratedBuildOutput(
            BuildLine: buildLine,
            LinearCuts: linearCuts,
            CountCuts: countCuts
        );
    }

    private string GetBuildListText(TurnVane turnVane, CalculatedTV calculated)
    {
        string labelText = string.IsNullOrWhiteSpace(turnVane.Label)
            ? ""
            : turnVane.Label;

        string linerText = $"{turnVane.Liner.ToDisplayText()} Liner";

        string splitText = turnVane.SplitVanes > 1
            ? $" - Split [x{turnVane.SplitVanes}]"
            : "";

        string cheekText;

        if (turnVane.CheekA == turnVane.CheekB)
        {
            cheekText = $"{MathJC.RoundToSixteenth(turnVane.CheekA)}\" Cheeks";
        }
        else
        {
            cheekText = $"{MathJC.RoundToSixteenth(turnVane.CheekA)}\" x {MathJC.RoundToSixteenth(turnVane.CheekB)}\" Cheeks";
        }

        return $"{labelText} - {turnVane.Qty}x) {calculated.Count} @ {MathJC.RoundToSixteenth(calculated.Length)}\"{splitText} - [{cheekText} - {MathJC.RoundToSixteenth(turnVane.Heel)}\" Heel - {linerText}]";
    }

}