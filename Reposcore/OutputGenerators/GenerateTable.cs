
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleTables;

public class GenerateTable
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

    public GenerateTable(Dictionary<string, UserScore> scores, string repoName, string folderPath)
    {
        _scores = scores;
        _repoName = repoName;
        _folderPath = folderPath;
    }

    public void Generate()
    {
        // 출력할 파일 경로
        string filePath = Path.Combine(_folderPath, $"{_repoName}1.txt");

        // 테이블 생성
        var headers = "UserId,f/b_PR,doc_PR,typo,f/b_issue,doc_issue,PR_rate,IS_rate,total".Split(',');

        // 각 칸의 너비 계산 (오른쪽 정렬을 위해 사용)
        int[] colWidths = headers.Select(h => h.Length).ToArray();

        var table = new ConsoleTable(headers);

        // 내용 작성
        foreach (var (id, scores) in _scores.OrderByDescending(x => x.Value.total))
        {
            double prRate = (sumOfPR > 0) ? (scores.PR_doc + scores.PR_fb + scores.PR_typo) / sumOfPR * 100 : 0.0;
            double isRate = (sumOfIs > 0) ? (scores.IS_doc + scores.IS_fb) / sumOfIs * 100 : 0.0;
            table.AddRow(
                id.PadRight(colWidths[0]), // 글자는 왼쪽 정렬                   
                scores.PR_fb.ToString().PadLeft(colWidths[1]), // 숫자는 오른쪽 정렬
                scores.PR_doc.ToString().PadLeft(colWidths[2]),
                scores.PR_typo.ToString().PadLeft(colWidths[3]),
                scores.IS_fb.ToString().PadLeft(colWidths[4]),
                scores.IS_doc.ToString().PadLeft(colWidths[5]),
                $"{prRate:F1}".PadLeft(colWidths[6]),
                $"{isRate:F1}".PadLeft(colWidths[7]),
                scores.total.ToString().PadLeft(colWidths[8])
            );
        }
        
        // 점수 기준 주석과 테이블 같이 출력
        var tableText = table.ToMinimalString();

        // 생성 정보 로그 계산
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var totals = _scores.Values.Select(s => s.total).ToList();
        double avg = totals.Count > 0 ? totals.Average() : 0.0;
        double max = totals.Count > 0 ? totals.Max() : 0.0;
        double min = totals.Count > 0 ? totals.Min() : 0.0;
        string metaLine = $"# Repo: {_repoName}  Date: {now}  Avg: {avg:F1}  Max: {max:F1}  Min: {min:F1}";

        var content = "# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1"
                    + Environment.NewLine
                    + metaLine
                    + Environment.NewLine
                    + tableText;

        File.WriteAllText(filePath, content);
        Console.WriteLine($"{filePath} 생성됨");
    }
}
