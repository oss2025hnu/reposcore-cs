
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
        // 1) 통계 계산
        var (avg, max, min) = CalcStats();
        // 2) 헤더 문자열
        string header =
            "# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1"
        + Environment.NewLine
        + $"# Repo: {_repoName}  Avg:{avg:F1}  Max:{max:F1}  Min:{min:F1}  참여자:{_scores.Count}명";

        // 3) 템플릿 메서드 호출
        GenerateOutput(".csv", header, writer =>
        {
            // --- 여기 한 줄만 추가하세요 (컬럼명) ---
            writer.WriteLine("User,GitHubProfile,f/b_PR,doc_PR,typo,f/b_issue,doc_issue,PR_rate,IS_rate,total");
            double sumPr = sumOfPR, sumIs = sumOfIs;
            foreach (var (id, s) in _scores.OrderByDescending(x => x.Value.total))
            {
                double prRate = sumPr > 0 ? (s.PR_doc + s.PR_fb + s.PR_typo) / sumPr * 100 : 0;
                double isRate = sumIs > 0 ? (s.IS_doc + s.IS_fb) / sumIs * 100 : 0;
                string profileUrl = $"https://github.com/{id}";
                writer.WriteLine(
                $"{id},{profileUrl},{s.PR_fb},{s.PR_doc},{s.PR_typo}," +
                $"{s.IS_fb},{s.IS_doc},{prRate:F1},{isRate:F1},{s.total}"
                );
            }
        });
    }

    private (double avg, double max, double min) CalcStats() 
    {
        var list = _scores.Values.Select(s => s.total).ToList();
        double avg = list.Any() ? list.Average() : 0;
        double max = list.Any() ? list.Max()     : 0;
        double min = list.Any() ? list.Min()     : 0;
        return (avg, max, min);
    }


    private void GenerateOutput(string ext, string header, Action<TextWriter> body)
    {
        EnsureDir();
        string path = GetPath(ext);
        using var writer = new StreamWriter(path);
        writer.WriteLine(header);
        body(writer);
        Console.WriteLine($"{path} 생성됨");
    }

        private void EnsureDir() 
    {
        if (!Directory.Exists(_folderPath))
            Directory.CreateDirectory(_folderPath);
    }

        private string GetPath(string ext) =>
        Path.Combine(_folderPath, $"{_repoName}{ext}");

}
