
using System;
using System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;

public class GenerateHtml
{
    private readonly string _repoName;
    private readonly string _folderPath;
    private static List<(string RepoName, Dictionary<string, UserScore> Scores)> _allRepos = new();
    public GenerateHtml(string repoName, string folderPath)
    {
        _repoName = repoName;
        _folderPath = folderPath;
    }

    public void Generate()
    {
        string filePath = Path.Combine(Path.GetDirectoryName(_folderPath)!, "index.html");
        using StreamWriter writer = new StreamWriter(filePath);

        // HTML 헤더 및 스타일
        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html lang='ko'>");
        writer.WriteLine("<head>");
        writer.WriteLine("    <meta charset='UTF-8'>");
        writer.WriteLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        writer.WriteLine("    <title>Reposcore Analysis</title>");
        writer.WriteLine("    <style>");
        writer.WriteLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
        writer.WriteLine("        .score-info { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; }");
        writer.WriteLine("        table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
        writer.WriteLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }");
        writer.WriteLine("        th { background-color: #f2f2f2; text-align: center; }");
        writer.WriteLine("        tr:nth-child(even) { background-color: #f9f9f9; }");
        writer.WriteLine("        tr:hover { background-color: #f5f5f5; }");
        writer.WriteLine("        .total { font-weight: bold; }");
        writer.WriteLine("        .tab { overflow: hidden; border: 1px solid #ccc; background-color: #f1f1f1; }");
        writer.WriteLine("        .tab button { background-color: inherit; float: left; border: none; outline: none; cursor: pointer; padding: 14px 16px; transition: 0.3s; }");
        writer.WriteLine("        .tab button:hover { background-color: #ddd; }");
        writer.WriteLine("        .tab button.active { background-color: #ccc; }");
        writer.WriteLine("        .tabcontent { display: none; padding: 6px 12px; border: 1px solid #ccc; border-top: none; }");
        writer.WriteLine("    </style>");
        writer.WriteLine("</head>");
        writer.WriteLine("<body>");

        // 점수 계산 기준 정보
        writer.WriteLine("    <div class='score-info'>");
        writer.WriteLine("        <h2>점수 계산 기준</h2>");
        writer.WriteLine("        <ul>");
        writer.WriteLine("            <li>PR_fb: 3점</li>");
        writer.WriteLine("            <li>PR_doc: 2점</li>");
        writer.WriteLine("            <li>PR_typo: 1점</li>");
        writer.WriteLine("            <li>IS_fb: 2점</li>");
        writer.WriteLine("            <li>IS_doc: 1점</li>");
        writer.WriteLine("        </ul>");
        writer.WriteLine("    </div>");

        // 탭 버튼 - Total을 첫 번째로 이동
        writer.WriteLine("    <div class='tab'>");
        writer.WriteLine("        <button class='tablinks active' onclick=\"openTab(event, 'total')\">Total</button>");
        foreach (var (repoName, _) in _allRepos)
        {
            writer.WriteLine($"        <button class='tablinks' onclick=\"openTab(event, '{repoName}')\">{repoName}</button>");
        }
        writer.WriteLine("    </div>");

        // 각 저장소별 탭 내용
        foreach (var (repoName, scores) in _allRepos)
        {
            writer.WriteLine($"    <div id='{repoName}' class='tabcontent'>");
            writer.WriteLine($"        <p>참여자 수: {scores.Count}명</p>"); //참여자 수 출력 추
            writer.WriteLine("        <table>");
            writer.WriteLine("            <thead>");
            writer.WriteLine("                <tr>");
            writer.WriteLine("                    <th>순위</th>");
            writer.WriteLine("                    <th>User</th>");
            writer.WriteLine("                    <th>f/b_PR</th>");
            writer.WriteLine("                    <th>doc_PR</th>");
            writer.WriteLine("                    <th>typo</th>");
            writer.WriteLine("                    <th>f/b_issue</th>");
            writer.WriteLine("                    <th>doc_issue</th>");
            writer.WriteLine("                    <th>PR_rate</th>");
            writer.WriteLine("                    <th>IS_rate</th>");
            writer.WriteLine("                    <th>total</th>");
            writer.WriteLine("                </tr>");
            writer.WriteLine("            </thead>");
            writer.WriteLine("            <tbody>");

            double repoSumOfPR = scores.Sum(pair => pair.Value.PR_doc + pair.Value.PR_fb + pair.Value.PR_typo);
            double repoSumOfIs = scores.Sum(pair => pair.Value.IS_doc + pair.Value.IS_fb);

            int currentRank = 1; // 순위
            double previousTotal = -1; // 이전 점수
            int position = 0; // 현재 위치

            foreach (var (id, score) in scores.OrderByDescending(x => x.Value.total))
            {
                position++;

                // 이전 점수와 다르면 현재 순위 업데이트
                if (score.total != previousTotal)
                {
                    currentRank = position;
                }

                double prRate = (repoSumOfPR > 0) ? (score.PR_doc + score.PR_fb + score.PR_typo) / repoSumOfPR * 100 : 0.0;
                double isRate = (repoSumOfIs > 0) ? (score.IS_doc + score.IS_fb) / repoSumOfIs * 100 : 0.0;

                writer.WriteLine("                <tr>");
                writer.WriteLine($"                    <td class='rank'>{currentRank}</td>");
                writer.WriteLine($"                    <td>{id}</td>");
                writer.WriteLine($"                    <td>{score.PR_fb}</td>");
                writer.WriteLine($"                    <td>{score.PR_doc}</td>");
                writer.WriteLine($"                    <td>{score.PR_typo}</td>");
                writer.WriteLine($"                    <td>{score.IS_fb}</td>");
                writer.WriteLine($"                    <td>{score.IS_doc}</td>");
                writer.WriteLine($"                    <td>{prRate:F1}%</td>");
                writer.WriteLine($"                    <td>{isRate:F1}%</td>");
                writer.WriteLine($"                    <td class='total'>{score.total}</td>");
                writer.WriteLine("                </tr>");

                // 이전 점수 업데이트
                previousTotal = score.total;
            }

            writer.WriteLine("            </tbody>");
            writer.WriteLine("        </table>");
            writer.WriteLine("    </div>");
        }

        // Total 탭 내용
        var totalScores = new Dictionary<string, UserScore>();
        foreach (var (_, scores) in _allRepos)
        {
            foreach (var (user, score) in scores)
            {
                if (!totalScores.ContainsKey(user))
                    totalScores[user] = score;
                else
                {
                    var prev = totalScores[user];
                    totalScores[user] = new UserScore(
                        prev.PR_fb + score.PR_fb,
                        prev.PR_doc + score.PR_doc,
                        prev.PR_typo + score.PR_typo,
                        prev.IS_fb + score.IS_fb,
                        prev.IS_doc + score.IS_doc,
                        prev.total + score.total
                    );
                }
            }
        }

        writer.WriteLine("    <div id='total' class='tabcontent'>");
        writer.WriteLine("        <table>");
        writer.WriteLine("            <thead>");
        writer.WriteLine("                <tr>");
        writer.WriteLine("                    <th>순위</th>");
        writer.WriteLine("                    <th>User</th>");
        writer.WriteLine("                    <th>f/b_PR</th>");
        writer.WriteLine("                    <th>doc_PR</th>");
        writer.WriteLine("                    <th>typo</th>");
        writer.WriteLine("                    <th>f/b_issue</th>");
        writer.WriteLine("                    <th>doc_issue</th>");
        writer.WriteLine("                    <th>total</th>");
        writer.WriteLine("                </tr>");
        writer.WriteLine("            </thead>");
        writer.WriteLine("            <tbody>");

        int totalCurrentRank = 1;
        double totalPreviousTotal = -1;
        int totalPosition = 0;

        foreach (var (id, score) in totalScores.OrderByDescending(x => x.Value.total))
        {
            totalPosition++;

            // 이전 점수와 다르면 현재 순위 업데이트
            if (score.total != totalPreviousTotal)
            {
                totalCurrentRank = totalPosition;
            }

            writer.WriteLine("                <tr>");
            writer.WriteLine($"                    <td class='rank'>{totalCurrentRank}</td>");
            writer.WriteLine($"                    <td>{id}</td>");
            writer.WriteLine($"                    <td>{score.PR_fb}</td>");
            writer.WriteLine($"                    <td>{score.PR_doc}</td>");
            writer.WriteLine($"                    <td>{score.PR_typo}</td>");
            writer.WriteLine($"                    <td>{score.IS_fb}</td>");
            writer.WriteLine($"                    <td>{score.IS_doc}</td>");
            writer.WriteLine($"                    <td class='total'>{score.total}</td>");
            writer.WriteLine("                </tr>");

            // 이전 점수 업데이트
            totalPreviousTotal = score.total;
        }

        writer.WriteLine("            </tbody>");
        writer.WriteLine("        </table>");
        writer.WriteLine("    </div>");

        // JavaScript for tab functionality
        writer.WriteLine("    <script>");
        writer.WriteLine("        function openTab(evt, tabName) {");
        writer.WriteLine("            var i, tabcontent, tablinks;");
        writer.WriteLine("            tabcontent = document.getElementsByClassName('tabcontent');");
        writer.WriteLine("            for (i = 0; i < tabcontent.length; i++) {");
        writer.WriteLine("                tabcontent[i].style.display = 'none';");
        writer.WriteLine("            }");
        writer.WriteLine("            tablinks = document.getElementsByClassName('tablinks');");
        writer.WriteLine("            for (i = 0; i < tablinks.length; i++) {");
        writer.WriteLine("                tablinks[i].className = tablinks[i].className.replace(' active', '');");
        writer.WriteLine("            }");
        writer.WriteLine("            document.getElementById(tabName).style.display = 'block';");
        writer.WriteLine("            evt.currentTarget.className += ' active';");
        writer.WriteLine("        }");
        // 첫 번째 탭을 기본으로 열기
        writer.WriteLine("        document.getElementsByClassName('tablinks')[0].click();");
        writer.WriteLine("    </script>");

        writer.WriteLine("</body>");
        writer.WriteLine("</html>");

        Console.WriteLine($"✅ HTML 보고서 생성 완료: {filePath}");
    }

}
