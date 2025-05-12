using Cocona;
using System;
using Octokit;

CoconaApp.Run((
    [Argument] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option("o|output", Description = "출력 디렉토리 경로를 지정합니다.")] string? output,
    [Option("f|format", Description = "출력 형식을 지정합니다.")] string? format,
    [Option('t', Description = "GitHub Personal Access Token 입력")] string? token // 토큰 입력 옵션 추가
) =>
{
    if (repos.Length != 2)
    {
        Console.WriteLine("! repository 인자는 'owner repo' 순서로 2개가 필요합니다.");
        Environment.Exit(1);  // 오류 발생 시 exit code 1로 종료
        return;
    }

    string owner = repos[0];
    string repo = repos[1];
    
    Console.WriteLine($"Repository: {String.Join("\n ", repos)}");

    if (verbose)
    {
        Console.WriteLine("Verbose mode is enabled.");
    }

    try
    {
        var client = new GitHubClient(new ProductHeaderValue("CoconaApp"));

        // GitHub Personal Access Token이 입력되었으면 인증 설정
        if (!string.IsNullOrEmpty(token))
        {
            client.Credentials = new Credentials(token);
        }

        // Repository 정보 가져오기
        var repository = client.Repository.Get(owner, repo).GetAwaiter().GetResult();

        // Repository에 대한 기본 정보 출력
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
    {if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("Error: --token 옵션을 입력해야 합니다."); // 토큰이 부족하면 오류 발생
    return; // 방송종료
}

        var analyzer = new GitHubAnalyzer(token); // 토큰을 전달
        analyzer.Analyze(repos[0], repos[1]);   // PR과 이슈 분석을 실행
    }
    catch (Exception ex)
    {
        Console.WriteLine($"! 오류 발생: {ex.Message}");
        Environment.Exit(1);  // 예외 발생 시 exit code 1로 종료
    }

    Environment.Exit(0);  // 정상 종료 시 exit code 0으로 종료
});
