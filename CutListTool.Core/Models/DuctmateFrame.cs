namespace CutListTool.Core.Models;

public readonly record struct DuctmateFrame(
    decimal Width,
    decimal Height,
    int Qty,
    string? Label = null
) : IBuildItem
{
    public BuildItemType BuildType => BuildItemType.Ductmate;

    public string GetBuildListText()
    {
        string labelText = string.IsNullOrWhiteSpace(Label) ? "" : $" - {Label}";

        return $"{Qty}x) {Width}\" x {Height}\"{labelText}";
    }
}
