namespace CutListTool.Core.Models;

public interface IBuildItem
{
    BuildItemType Type { get; }

    string GetBuildListText();
}