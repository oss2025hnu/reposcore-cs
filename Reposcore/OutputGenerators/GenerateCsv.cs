
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GenerateCsv
{
    private readonly Dictionary<string, UserScore> _scores;
    private readonly string _repoName;
    private readonly string _folderPath;

    double sumOfPR
    {
        get
        {
            return _scores.Sum(pair => pair.Value.PR_doc + pair.Value.PR_fb + pair.Value.PR_typo);
        }        
    }

    double sumOfIs
    {
        get { return _scores.Sum(pair => pair.Value.IS_doc + pair.Value.IS_fb); }
    }


    public GenerateCsv(Dictionary<string, UserScore> scores, string repoName, string folderPath)
    {
        _scores = scores;
        _repoName = repoName;
        _folderPath = folderPath;
    }

    public void Generate()
    {
        // 경로 설정
        string filePath = Path.Combine(_folderPath, $"{_repoName}.csv");
        using StreamWriter writer = new StreamWriter(filePath);

        
        // 파일에 "# 점수 계산 기준…" 을 쓰면, 이 줄이 CSV 첫 줄로 나옵니다.
        writer.WriteLine("# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1");
        // CSV 헤더
        writer.WriteLine("User,f/b_PR,doc_PR,typo,f/b_issue,doc_issue,PR_rate,IS_rate,total");

        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var totals = _scores.Values.Select(s => s.total).ToList();
        double avg = totals.Count > 0 ? totals.Average() : 0.0;
        double max = totals.Count > 0 ? totals.Max() : 0.0;
        double min = totals.Count > 0 ? totals.Min() : 0.0;
        writer.WriteLine($"# Repo: {_repoName}  Date: {now}  Avg: {avg:F1}  Max: {max:F1}  Min: {min:F1}");

        // 내용 작성
        foreach (var (id, scores) in _scores.OrderByDescending(x => x.Value.total))
        {
            double prRate = (sumOfPR > 0) ? (scores.PR_doc + scores.PR_fb + scores.PR_typo) / sumOfPR * 100 : 0.0;
            double isRate = (sumOfIs > 0) ? (scores.IS_doc + scores.IS_fb) / sumOfIs * 100 : 0.0;
            string line = $"{id},{scores.PR_fb},{scores.PR_doc},{scores.PR_typo},{scores.IS_fb},{scores.IS_doc},{prRate:F1},{isRate:F1},{scores.total}";
            writer.WriteLine(line);
        }

        Console.WriteLine($"{filePath} 생성됨");
        }
}
