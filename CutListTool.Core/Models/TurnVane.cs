namespace CutListTool.Core.Models;

public readonly record struct TurnVane(
    decimal CheekA,
    decimal CheekB,
    decimal Heel,    
    int Qty,
    string? Label = null
): IBuildItem
{
    public BuildItemType Type => BuildItemType.TurnVane;

    public TurnVane(
        decimal cheekA,
        decimal heel,
        int qty,
        string? label = null
    ) : this(
        CheekA : cheekA,
        CheekB: cheekA,
        Heel: heel,
        Qty: qty,
        Label: label
    )
    {
        
    }

    public string GetBuildListText()
    {
        string labelText = string.IsNullOrWhiteSpace(Label) ? "" : Label;

        return $"{Label} - ";
    }
}