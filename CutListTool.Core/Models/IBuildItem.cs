namespace CutListTool.Core.Models;

public interface IBuildItem
{
    BuildItemType BuildType { get; }

    string GetBuildListText();
}