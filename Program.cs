using Cocona;
// using System;
//using System.Threading.Tasks;

CoconaApp.Run((
    [Argument] string[] repository,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose
    [Option('o', "output", Description = "출력 디렉토리 경로를 지정합니다.")] string output  // --output 옵션 
    [Option('f', "format", Description = "출력 형식을 지정합니다.", DefaultValue = "text")] string format  // --format 옵션 (기본값 'text')
    [Option("check-limit", Description = "한계 체크 여부를 설정합니다.")] bool checkLimit  // --check-limit 옵션
    [Option("user-info", Description = "사용자 정보 파일 경로를 지정합니다.")] string userInfo  // --user-info 옵션
) =>
{
    Console.WriteLine($"Repository: {String.Join("\n ", repository)}");

    if (verbose)
    {
        Console.WriteLine("Verbose mode is enabled.");
    }

    if (repository.Length != 2)
    {
        Console.WriteLine("❗ repository 인자는 'owner repo' 순서로 2개가 필요합니다.");
        Environment.Exit(1);  // 오류 발생 시 exit code 1로 종료
        return;
    }
    Console.WriteLine($"verbose: {verbose}");
    Console.WriteLine($"output: {(output ?? "지정 안됨")}");
    Console.WriteLine($"format: {format}");
    Console.WriteLine($"check-limit: {checkLimit}");
    Console.WriteLine($"user-info: {(userInfo ?? "지정 안됨")}");
    try
    {
        // var analyzer = new GitHubAnalyzer();
        // analyzer.Analyze(repository[0], repository[1]);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❗ 오류 발생: {ex.Message}");
        Environment.Exit(1);  // 예외 발생 시 exit code 1로 종료
    }

    Environment.Exit(0);  // 정상 종료 시 exit code 0으로 종료
});
