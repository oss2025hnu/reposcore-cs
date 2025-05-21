using Cocona;
using System;
using System.Collections.Generic;
using Octokit;

CoconaApp.Run((
    [Argument] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option('t', Description = "GitHub Personal Access Token 입력")] string? token,
    [Option('o', Description = "출력 디렉토리 경로를 지정합니다. (기본값: output)")] string output = "output",
    [Option('f', Description = "출력 형식을 지정합니다. (기본값: json)")] string format = "json"
) =>
{
    // 더미 데이타가 실제로 불러와 지는지 기본적으로 확인하기 위한 코드
    var repo1Activities = DummyData.repo1Activities;
    Console.WriteLine("repo1Activities:"+repo1Activities.Count);
    var repo2Activities = DummyData.repo2Activities;
    Console.WriteLine("repo2Activities:"+repo2Activities.Count);

    if (repos.Length != 2)
    {
        Console.WriteLine("! repository 인자는 'owner repo' 순서로 2개가 필요합니다.");
        Environment.Exit(1);
        return;
    }

    string owner = repos[0];
    string repo = repos[1];

    Console.WriteLine($"Repository: {string.Join("\n ", repos)}");

    if (verbose)
    {
        Console.WriteLine("Verbose mode is enabled.");
    }

    try
    {
        var client = new GitHubClient(new ProductHeaderValue("CoconaApp"));

        if (!string.IsNullOrEmpty(token))
        {
            client.Credentials = new Credentials(token);
        }

        var repository = client.Repository.Get(owner, repo).GetAwaiter().GetResult();

        Console.WriteLine($"[INFO] Repository Name: {repository.Name}");
        Console.WriteLine($"[INFO] Full Name: {repository.FullName}");
        Console.WriteLine($"[INFO] Description: {repository.Description}");
        Console.WriteLine($"[INFO] Stars: {repository.StargazersCount}");
        Console.WriteLine($"[INFO] Forks: {repository.ForksCount}");
        Console.WriteLine($"[INFO] Open Issues: {repository.OpenIssuesCount}");
        Console.WriteLine($"[INFO] Language: {repository.Language}");
        Console.WriteLine($"[INFO] URL: {repository.HtmlUrl}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"! 오류 발생: {e.Message}");
        Environment.Exit(1);
    }

    try
    {
        var formats = string.IsNullOrWhiteSpace(format)
            ? new List<string> { "json" }
            : new List<string>(format.Split(',', StringSplitOptions.RemoveEmptyEntries));

        var analyzer = new GitHubAnalyzer(token!); // ✅ null-forgiving 연산자 적용
        analyzer.Analyze(owner, repo, output, formats); // outputDir에서 output으로 수정

    }
    catch (Exception ex)
    {
        Console.WriteLine($"! 오류 발생: {ex.Message}");
        Environment.Exit(1);
    }

    Environment.Exit(0);
});
