using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GitHubAnalyzer
{
    private readonly GitHubClient _client;

    public GitHubAnalyzer()
    {
        _client = new GitHubClient(new ProductHeaderValue("reposcore-cs"));
    }

    public async Task Analyze(string owner, string repo)
    {
        // Merged PR 수집
        var prs = await _client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest
        {
            State = ItemStateFilter.Closed
        });

        // Open + Closed 이슈 수집
        var issues = await _client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest
        {
            State = ItemStateFilter.All
        });

        // 통계 초기화
        int p_fb = 0, p_d = 0, p_t = 0;
        int i_fb = 0, i_d = 0;

        foreach (var pr in prs.Where(p => p.Merged == true))
        {
            if (IsDoc(pr.Title) || IsDoc(pr.Body)) p_d++;
            else if (IsTypo(pr.Title) || IsTypo(pr.Body)) p_t++;
            else p_fb++;
        }

        foreach (var issue in issues)
        {
            if (issue.PullRequest != null) continue;

            if (IsDoc(issue.Title) || IsDoc(issue.Body)) i_d++;
            else i_fb++;
        }

        // 결과 출력
        Console.WriteLine("\n📊 통계 결과");
        Console.WriteLine($"기능/버그 PR (P_fb): {p_fb}");
        Console.WriteLine($"문서 PR (P_d): {p_d}");
        Console.WriteLine($"오타 PR (P_t): {p_t}");
        Console.WriteLine($"기능/버그 이슈 (I_fb): {i_fb}");
        Console.WriteLine($"문서 이슈 (I_d): {i_d}");
    }

    private bool IsDoc(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        var lower = text.ToLower();
        return lower.Contains("docs") || lower.Contains("documentation") || lower.Contains("readme");
    }

    private bool IsTypo(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        var lower = text.ToLower();
        return lower.Contains("typo") || lower.Contains("오타");
    }
}
