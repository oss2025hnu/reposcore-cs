using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleTables;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;

public class FileGenerator
{
    private readonly Dictionary<string, UserScore> _scores;
    private readonly string _repoName;
    private readonly string _folderPath;

    public FileGenerator(Dictionary<string, UserScore> repoScores, string repoName, string folderPath)
    {
        _scores = repoScores;
        _repoName = repoName;
        _folderPath = Path.Combine(folderPath, repoName);
        Directory.CreateDirectory(_folderPath);
    }

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

    public void GenerateCsv()
    {
        // 경로 설정
        string filePath = Path.Combine(_folderPath, $"{_repoName}.csv");
        using StreamWriter writer = new StreamWriter(filePath);

        
        // 파일에 "# 점수 계산 기준…" 을 쓰면, 이 줄이 CSV 첫 줄로 나옵니다.
        writer.WriteLine("# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1");
        // CSV 헤더
        writer.WriteLine("User,f/b_PR,doc_PR,typo,f/b_issue,doc_issue,PR_rate,IS_rate,total");

        // 내용 작성
        foreach (var (id, scores) in _scores.OrderByDescending(x => x.Value.total))
        {
            double prScore = scores.PR_fb * 3 + scores.PR_doc * 2 + scores.PR_typo * 1;
            double isScore = scores.IS_fb * 2 + scores.IS_doc * 1;
            double total = prScore + isScore;
            double prRate = (total > 0) ? prScore / total * 100 : 0.0;
            double isRate = (total > 0) ? isScore / total * 100 : 0.0;
            string line =
                $"{id},{scores.PR_fb},{scores.PR_doc},{scores.PR_typo},{scores.IS_fb},{scores.IS_doc},{prRate:F1}%,{isRate:F1}%,{scores.total}";
            writer.WriteLine(line);
        }

        Console.WriteLine($"{filePath} 생성됨");
    }
    public void GenerateTable()
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
            double prScore = scores.PR_fb * 3 + scores.PR_doc * 2 + scores.PR_typo * 1;
            double isScore = scores.IS_fb * 2 + scores.IS_doc * 1;
            double total = prScore + isScore;
            double prRate = (total > 0) ? prScore / total * 100 : 0.0;
            double isRate = (total > 0) ? isScore / total * 100 : 0.0;
            string totalWithRate = $"{scores.total} [PR:{prRate:F1}%, Issue:{isRate:F1}%]";
            table.AddRow(
                id.PadRight(colWidths[0]), // 글자는 왼쪽 정렬                   
                scores.PR_fb.ToString().PadLeft(colWidths[1]), // 숫자는 오른쪽 정렬
                scores.PR_doc.ToString().PadLeft(colWidths[2]),
                scores.PR_typo.ToString().PadLeft(colWidths[3]),
                scores.IS_fb.ToString().PadLeft(colWidths[4]),
                scores.IS_doc.ToString().PadLeft(colWidths[5]),
                $"{prRate:F1}%".PadLeft(colWidths[6]),
                $"{isRate:F1}%".PadLeft(colWidths[7]),
                scores.total.ToString().PadLeft(colWidths[8])
            );
        }
        
        // 점수 기준 주석과 테이블 같이 출력
        var tableText = table.ToMinimalString();
        var content = "# 점수 계산 기준: PR_fb*3, PR_doc*2, PR_typo*1, IS_fb*2, IS_doc*1"
                    + Environment.NewLine
                    + tableText;
        File.WriteAllText(filePath, content);
        Console.WriteLine($"{filePath} 생성됨");
    }

    public void GenerateChart()
    {
        var labels = new List<string>();
        var values = new List<double>();

        foreach (var (user, score) in _scores.OrderBy(x => x.Value.total)) // 오름차순
        {
            labels.Add(user);
            values.Add(score.total);
        }

        string[] names = labels.ToArray();
        double[] scores = values.ToArray();
        
        // ✅ 간격 조절된 Position
        double spacing = 10; // 막대 간격
        double[] positions = Enumerable.Range(0, names.Length)
                                    .Select(i => i * spacing)
                                    .ToArray();

        // Bar 데이터 생성
        var bars = new List<Bar>();
        for (int i = 0; i < scores.Length; i++)
        {
            bars.Add(new Bar
            {
                Position = positions[i],
                Value = scores[i],
                FillColor = Colors.SteelBlue,
                Orientation = Orientation.Horizontal,
                Size = 5,
            });
        }

        var plt = new ScottPlot.Plot();
        var barPlot = plt.Add.Bars(bars);

        plt.Axes.Left.TickGenerator = new NumericManual(positions, names);
        plt.Title($"Scores - {_repoName}");
        plt.XLabel("총 점수");
        plt.YLabel("사용자");

        // x축 범위 설정
        plt.Axes.Bottom.Min = 0;
        plt.Axes.Bottom.Max = scores.Max() * 1.1; // 최대값의 110%까지 표시

        string outputPath = Path.Combine(_folderPath, $"{_repoName}_chart.png");
        plt.SavePng(outputPath, 1920, 1080);
        Console.WriteLine($"✅ 차트 생성 완료: {outputPath}");
    }


}
