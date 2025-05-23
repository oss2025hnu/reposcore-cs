using Cocona;
using System;
using System.Collections.Generic;
using Octokit;

CoconaApp.Run((
    [Argument] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option('o', Description = "출력 디렉토리 경로를 지정합니다.")] string? output,
    [Option('t', Description = "GitHub Personal Access Token 입력")] string? token,
    [Option('f', Description = "출력 형식을 지정합니다. (text, csv, chart, html, all)")] string format = "all"
) =>
{
    var supportedFormats = new HashSet<string> { "json", "csv" };

    if (!supportedFormats.Contains(format.ToLowerInvariant()))
    {
        Console.WriteLine($"지원되지 않는 출력 형식입니다: {format}");
        Console.WriteLine("지원 형식: json, csv");
        Environment.Exit(1);
    }

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
        var formats = getValidFormat(format);

        var outputDir = string.IsNullOrWhiteSpace(output) ? "output" : output;

        var dataCollector = new RepoDataCollector(token!); // ✅ null-forgiving 연산자 적용
        dataCollector.Collect(owner, repo, outputDir, formats);

        // ===== 파일 생성 기능 구현 후 제거 =====
        Console.WriteLine("\n===생성되는 포맷===");
        foreach (var fm in formats)
        {
            Console.WriteLine($"-{fm}");
        }
        Console.WriteLine("\n파일 생성 기능이 아직 구현되지 않았습니다.");
        // ===== 파일 생성 기능 구현 후 제거 =====
    }
    catch (Exception ex)
    {
        Console.WriteLine($"! 오류 발생: {ex.Message}");
        Environment.Exit(1);
    }

    Environment.Exit(0);
});

static List<string> getValidFormat(string format) 
{   
    var formats = new List<string>(format.Split(',', StringSplitOptions.RemoveEmptyEntries)); 
    var FormatList = new List<string> {"text", "csv", "chart", "html", "all"}; // 유효한 format

    var validFormats = new List<string> { };
    var unValidFormats = new List<string> { };

    foreach (var fm in formats)
    {
        if (FormatList.Contains(fm)) validFormats.Add(fm);
        else unValidFormats.Add(fm);
    }

    // 유효하지 않은 포맷이 존재
    if (unValidFormats.Count != 0)
    {   
        Console.WriteLine("유효하지 않은 포맷이 존재합니다.");
        Console.Write("유효하지 않은 포맷: ");
        foreach (var unValidFormat in unValidFormats)
        {
            Console.Write($"{unValidFormat} ");
        }
        Console.Write("\n");
    }

    // 유효한 포맷이 존재 X
    if (validFormats.Count == 0)
    {
        Console.WriteLine("유효한 포맷이 존재하지 않습니다.");
        Console.WriteLine("모든 포맷을 생성합니다.");
        validFormats.Add("all");
    }

    // 추출한 리스트에에 "all"이 존재할 경우 모든 포맷 리스트 반환
    if (validFormats.Contains("all"))
    {
        return new List<string> { "text", "csv", "chart", "html" };
    }

    return validFormats;
}