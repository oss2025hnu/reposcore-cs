
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using Alignment = ScottPlot.Alignment;

public class GenerateChart
{
    private readonly Dictionary<string, UserScore> _scores;
    private readonly string _repoName;
    private readonly string _folderPath;

    public GenerateChart(Dictionary<string, UserScore> scores, string repoName, string folderPath)
    {
        _scores = scores;
        _repoName = repoName;
        _folderPath = folderPath;
    }


    public void Generate()
    {
        var labels = new List<string>();
        var values = new List<double>();

        // total 점수 내림차순 정렬
        var sorted = _scores.OrderByDescending(x => x.Value.total).ToList();
        var rankList = new List<(int Rank, string User, double Score)>();
        int rank = 1;
        int count = 1;
        double? prevScore = null;

        foreach (var pair in sorted)
        {
            if (prevScore != null && pair.Value.total != prevScore)
            {
                rank = count;
            }
            rankList.Add((rank, pair.Key, pair.Value.total));
            prevScore = pair.Value.total;
            count++;
        }

        // 차트는 오름차순으로 표시
        foreach (var item in rankList.OrderBy(x => x.Score))
        {
            string suffix = item.Rank switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
            labels.Add($"{item.User} ({item.Rank}{suffix})");
            values.Add(item.Score);
        }

        string[] names = labels.ToArray();
        double[] scores = values.ToArray();

        // ✅ 간격 조절된 Position
        double spacing = 10; // 막대 간격
        double[] positions = Enumerable.Range(0, names.Length)
                                    .Select(i => i * spacing)
                                    .ToArray();

        // Bar 데이터 생성
        var plt = new ScottPlot.Plot();
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

            double textX = scores[i] + scores.Max() * 0.01;
            double textY = positions[i];

            // 사용자 이름 추출 (labels와 rankList는 같은 순서)
            var userLabel = labels[i];
            // userLabel은 "user (1st)" 형태이므로, rankList에서 사용자 이름을 가져옴
            var userName = rankList.OrderBy(x => x.Score).ElementAt(i).User;
            if (_scores.TryGetValue(userName, out var userScore))
            {
                string detailText = $"{userScore.total} (P-F: {userScore.PR_fb}, D: {userScore.PR_doc}, T: {userScore.PR_typo} / I-F: {userScore.IS_fb}, D: {userScore.IS_doc})";
                var txt = plt.Add.Text(detailText, textX, textY);
                txt.Alignment = Alignment.MiddleLeft;
            }
            else
            {
                // 혹시라도 매칭이 안 될 경우 기존 총점만 표시
                var txt = plt.Add.Text($"{scores[i]:F1}", textX, textY);
                txt.Alignment = Alignment.MiddleLeft;
            }
        }

        var barPlot = plt.Add.Bars(bars);

        string now = GetKoreanTimeString();
        double avg = scores.Average();
        double max = scores.Max();
        double min = scores.Min();

        string chartTitle = $"Repo: {_repoName}  Date: {now}";
        plt.Axes.Left.TickGenerator = new NumericManual(positions, names);
        plt.Title($"Scores - {_repoName}" + "\n" + chartTitle);
        plt.XLabel("Total Score");
        plt.YLabel("User");

        // x축 범위 설정
        plt.Axes.Bottom.Min = 0;
        plt.Axes.Bottom.Max = scores.Max() * 2.5;

        // 통계 정보 추가
        double maxScore = scores.Max();
        double minScore = scores.Min();
        double avgScore = scores.Average();

        // 제목 근처에 통계 정보 표시
        double xRight = scores.Max() * 2.4; // x축 최대값의 90% 위치
        double yTop = positions[^3] + spacing * 2; // 마지막 막대 위에 표시
        double ySpacing = spacing * 0.8; // 텍스트 간격

        var maxText = plt.Add.Text($"max: {maxScore:F1}", xRight, yTop);
        maxText.Alignment = Alignment.UpperRight;
        maxText.LabelFontColor = Colors.DarkGreen;

        var avgText = plt.Add.Text($"avg: {avgScore:F1}", xRight, yTop - ySpacing);
        avgText.Alignment = Alignment.UpperRight;
        avgText.LabelFontColor = Colors.DarkBlue;

        var minText = plt.Add.Text($"min: {minScore:F1}", xRight, yTop - ySpacing * 2);
        minText.Alignment = Alignment.UpperRight;
        minText.LabelFontColor = Colors.DarkRed;

        string outputPath = Path.Combine(_folderPath, $"{_repoName}_chart.png");
        plt.SavePng(outputPath, 1080, 1920);
        Console.WriteLine($"✅ 차트 생성 완료: {outputPath}");
    }
    
        private static string GetKoreanTimeString()
    {
        TimeZoneInfo kstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
        DateTime nowKST = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kstZone);
        return nowKST.ToString("yyyy-MM-dd HH:mm");
    }
}
