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
        // 병합된 PR 수집
        var prs = await _client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest
        {
            State = ItemStateFilter.Closed
        });

        // 전체 이슈 수집
        var issues = await _client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest
        {
            State = ItemStateFilter.All
        });

        // 라벨 기준 통계 변수
        int pr_bug = 0, pr_doc = 0, pr_feat = 0;
        int issue_bug = 0, issue_doc = 0, issue_feat = 0;

        // PR 분류 (병합된 것만)
        foreach (var pr in prs.Where(p => p.Merged == true))
        {
            var labels = pr.Labels.Select(l => l.Name.ToLower()).ToList();

            if (labels.Contains("bug")) pr_bug++;
            if (labels.Contains("documentation")) pr_doc++;
            if (labels.Contains("enhancement")) pr_feat++;
        }

        // 이슈 분류 (PR 제외)
        foreach (var issue in issues)
        {
            if (issue.PullRequest != null) continue;

            var labels = issue.Labels.Select(l => l.Name.ToLower()).ToList();

            if (labels.Contains("bug")) issue_bug++;
            if (labels.Contains("documentation")) issue_doc++;
            if (labels.Contains("enhancement")) issue_feat++;
        }

        // 결과 출력
        Console.WriteLine("\n📊 GitHub Label 통계 결과");

        Console.WriteLine("\n✅ Pull Requests (Merged)");
        Console.WriteLine($"- Bug PRs: {pr_bug}");
        Console.WriteLine($"- Documentation PRs: {pr_doc}");
        Console.WriteLine($"- Enhancement PRs: {pr_feat}");

        Console.WriteLine("\n✅ Issues");
        Console.WriteLine($"- Bug Issues: {issue_bug}");
        Console.WriteLine($"- Documentation Issues: {issue_doc}");
        Console.WriteLine($"- Enhancement Issues: {issue_feat}");
    }
}
