namespace CutListTool.Core.Models;

public interface IBuildItem
{
    CutItemType Type { get; }

    string GetBuildListText();
}