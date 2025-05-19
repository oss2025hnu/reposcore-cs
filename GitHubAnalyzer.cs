using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GitHubAnalyzer
{
    private readonly GitHubClient _client;

    public GitHubAnalyzer(string token)
    {
        _client = CreateClient("reposcore-cs", token);
    }

    private GitHubClient CreateClient(string productName, string token)
    {
        var client = new GitHubClient(new ProductHeaderValue(productName));

        if (!string.IsNullOrEmpty(token))
        {
            client.Credentials = new Credentials(token);
        }

        return client;
    }

    private void HandleError(Exception ex)
    {
        Console.WriteLine($"❗ 알 수 없는 오류가 발생했습니다: {ex.Message}");
        Environment.Exit(1);
    }

    public void Analyze(string owner, string repo, string outputDir, List<string> formats)
    {
        try
        {
            Console.WriteLine("📥 Pull Requests 로딩 중...");
            var prs = _client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest
            {
                State = ItemStateFilter.Closed
            }).Result;

            Console.WriteLine("📥 Issues 로딩 중...");
            var issues = _client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest
            {
                State = ItemStateFilter.All
            }).Result;

            Console.WriteLine("🔍 라벨 통계 분석 중...");
            var targetLabels = new[] { "bug", "documentation", "enhancement" };
            var labelCounts = targetLabels.ToDictionary(label => label, _ => 0);

            foreach (var pr in prs.Where(p => p.Merged == true))
            {
                var labels = pr.Labels.Select(l => l.Name.ToLower()).ToList();
                foreach (var label in targetLabels)
                {
                    if (labels.Contains(label))
                        labelCounts[label]++;
                }
            }

            foreach (var issue in issues)
            {
                if (issue.PullRequest != null) continue;
                var labels = issue.Labels.Select(l => l.Name.ToLower()).ToList();
                foreach (var label in targetLabels)
                {
                    if (labels.Contains(label))
                        labelCounts[label]++;
                }
            }

            Console.WriteLine("\n📊 GitHub Label 통계 결과");

            Console.WriteLine("\n✅ Pull Requests (Merged)");
            foreach (var label in targetLabels)
            {
                Console.WriteLine($"- {char.ToUpper(label[0]) + label.Substring(1)} PRs: {labelCounts[label]}");
