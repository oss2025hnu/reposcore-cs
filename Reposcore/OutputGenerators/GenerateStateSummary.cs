
using System;
using System.IO;

public class GenerateStateSummary
{
    private readonly string _repoName;
    private readonly string _folderPath;

    public GenerateStateSummary( string repoName, string folderPath)
    {
        _repoName = repoName;
        _folderPath = folderPath;
    }

    public void Generate(RepoStateSummary summary)
    {
        string filePath = Path.Combine(_folderPath, $"{_repoName}_state.txt");
        using StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine($"Merged PR: {summary.MergedPR}");
        writer.WriteLine($"Unmerged PR: {summary.UnmergedPR}");
        writer.WriteLine($"Open Issue: {summary.OpenIssue}");
        writer.WriteLine($"Closed Issue: {summary.ClosedIssue}");
        Console.WriteLine($"{filePath} 생성됨");
    }
}
