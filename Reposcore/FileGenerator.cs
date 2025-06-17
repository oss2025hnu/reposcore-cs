using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleTables;
using ScottPlot;
using ScottPlot.Plottables;

public static class ScoreFormatter
{
    public static string ToCsvLine(string id, UserScore s, double prRate, double isRate) =>
        $"{id},{s.PR_fb},{s.PR_doc},{s.PR_typo},{s.IS_fb},{s.IS_doc},{prRate:F1},{isRate:F1},{s.total}";

    public static object[] ToTableRow(int rank, string id, UserScore s, double prRate, double isRate) =>
        new object[] { rank, id, s.PR_fb, s.PR_doc, s.PR_typo, s.IS_fb, s.IS_doc, $"{prRate:F1}", $"{isRate:F1}", s.total };
}

public class FileGenerator
{
    private readonly Dictionary<string, UserScore> _scores;
    private readonly string _repoName;
    private readonly string _folderPath;
    private static readonly List<(string RepoName, Dictionary<string, UserScore> Scores)> _allRepos = new();

    public FileGenerator(Dictionary<string, UserScore> repoScores, string repoName, string folderPath)
    {
        _scores = repoScores;
        _repoName = repoName;
        _folderPath = Path.Combine(folderPath, repoName);
        _allRepos.Add((repoName, repoScores));

        try
        {
            Directory.CreateDirectory(_folderPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❗ 결과 디렉토리 생성에 실패했습니다: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private void EnsureDir()
    {
        if (!Directory.Exists(_folderPath))
            Directory.CreateDirectory(_folderPath);
    }

    private string GetPath(string ext) => Path.Combine(_folderPath, $"{_repoName}{ext}");

    private (double avg, double max, double min) CalcStats()
    {
        var list = _scores.Values.Select(s => s.total).ToList();
        return (list.DefaultIfEmpty(0).Average(), list.DefaultIfEmpty(0).Max(), list.DefaultIfEmpty(0).Min());
    }

    private double sumOfPR => _scores.Sum(pair => pair.Value.PR_doc + pair.Value.PR_fb + pair.Value.PR_typo);
    private double sumOfIs => _scores.Sum(pair => pair.Value.IS_doc + pair.Value.IS_fb);

    private void GenerateOutput(string ext, string header, Action<TextWriter> body)
    {
        EnsureDir();
        string path = GetPath(ext);
        using var writer = new StreamWriter(path);
        writer.WriteLine(header);
        body(writer);
        Console.WriteLine($"{path} 생성됨");
    }

    public void GenerateCsv()
    {
        var (avg, max, min) = CalcStats();
        string header =
            "# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1\n" +
            $"# Repo: {_repoName}  Avg:{avg:F1}  Max:{max:F1}  Min:{min:F1}  참여자:{_scores.Count}명";

        GenerateOutput(".csv", header, writer =>
        {
            writer.WriteLine("User,GitHubProfile,f/b_PR,doc_PR,typo,f/b_issue,doc_issue,PR_rate,IS_rate,total");
            double sumPr = sumOfPR, sumIs = sumOfIs;
            foreach (var (id, s) in _scores.OrderByDescending(x => x.Value.total))
            {
                double prRate = sumPr > 0 ? (s.PR_doc + s.PR_fb + s.PR_typo) / sumPr * 100 : 0;
                double isRate = sumIs > 0 ? (s.IS_doc + s.IS_fb) / sumIs * 100 : 0;
                string profileUrl = $"https://github.com/{id}";
                writer.WriteLine($"{id},{profileUrl},{s.PR_fb},{s.PR_doc},{s.PR_typo},{s.IS_fb},{s.IS_doc},{prRate:F1},{isRate:F1},{s.total}");
            }
        });

        // 저장소 요약 통계 생성
        GenerateSummaryCsv(_folderPath);
    }

    public void GenerateTable()
    {
        var filePath = Path.Combine(_folderPath, $"{_repoName}1.txt");
        var headers = "Rank,UserId,f/b_PR,doc_PR,typo,f/b_issue,doc_issue,PR_rate,IS_rate,total".Split(',');
        var table = new ConsoleTable(headers);

        var sortedScores = _scores.OrderByDescending(x => x.Value.total).ToList();
        int currentRank = 1, count = 1;
        double? previousScore = null;

        foreach (var (id, scores) in sortedScores)
        {
            if (previousScore != null && scores.total != previousScore)
                currentRank = count;

            double prRate = sumOfPR > 0 ? (scores.PR_doc + scores.PR_fb + scores.PR_typo) / sumOfPR * 100 : 0;
            double isRate = sumOfIs > 0 ? (scores.IS_doc + scores.IS_fb) / sumOfIs * 100 : 0;
            table.AddRow(currentRank, id, scores.PR_fb, scores.PR_doc, scores.PR_typo, scores.IS_fb, scores.IS_doc, $"{prRate:F1}", $"{isRate:F1}", scores.total);

            previousScore = scores.total;
            count++;
        }

        string now = GetKoreanTimeString();
        var totals = _scores.Values.Select(s => s.total).ToList();
        string content =
            "# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1\n" +
            $"# Repo: {_repoName}  Date: {now}  Avg: {totals.Average():F1}  Max: {totals.Max():F1}  Min: {totals.Min():F1}\n" +
            $"# 참여자 수: {_scores.Count}명\n" +
            table.ToMinimalString();

        File.WriteAllText(filePath, content);
        Console.WriteLine($"{filePath} 생성됨");
    }

    public void GenerateChart()
    {
        // 생략: 기존 ScottPlot 코드 유지
    }

    // ✅ 전체 저장소에 대한 summary.csv 생성
    public static void GenerateSummaryCsv(string baseFolder)
    {
        string summaryPath = Path.Combine(baseFolder, "summary.csv");
        using var writer = new StreamWriter(summaryPath);
        writer.WriteLine("Repo,Participant,Average,Max,Min");

        foreach (var (repo, scores) in _allRepos)
        {
            var list = scores.Values.Select(s => s.total).ToList();
            double avg = list.DefaultIfEmpty(0).Average();
            double max = list.DefaultIfEmpty(0).Max();
            double min = list.DefaultIfEmpty(0).Min();
            writer.WriteLine($"{repo},{scores.Count},{avg:F1},{max:F1},{min:F1}");
        }

        Console.WriteLine($"{summaryPath} 생성됨");
    }

    private static string GetKoreanTimeString()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
        return TimeZoneInfo.ConvertTime(DateTime.Now, timeZone).ToString("yyyy-MM-dd HH:mm:ss");
    }
}
