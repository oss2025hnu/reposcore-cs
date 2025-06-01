﻿using Cocona;
using System;
using System.Collections.Generic;
using Octokit;
using DotNetEnv;


CoconaApp.Run((
    [Argument(Description = "분석할 저장소. \"owner/repo\" 형식으로 공백을 구분자로 하여 여러 개 입력")] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option('o', Description = "출력 디렉토리 경로를 지정합니다. (default : \"result\")")] string? output,
    [Option('f', Description = "출력 형식 지정 (\"text\", \"csv\", \"chart\", \"html\", \"all\", default : \"all\")")] string[]? format,
    [Option('t', Description = "GitHub 액세스 토큰 입력")] string? token,
    [Option("only-pr", Description = "PR 활동만 분석합니다.")] bool onlyPR,
    [Option("only-issue", Description = "이슈 활동만 분석합니다.")] bool onlyIssue
) =>
{
    //  옵션 충돌 방지: --only-pr 와 --only-issue는 동시에 사용 불가 
    if (onlyPR && onlyIssue)
    {
        Console.WriteLine("❌ --only-pr 와 --only-issue 옵션은 동시에 사용할 수 없습니다.");
        Environment.Exit(1);
    }
    // 더미 데이타가 실제로 불러와 지는지 기본적으로 확인하기 위한 코드
    var repo1Activities = DummyData.repo1Activities;
    Console.WriteLine("repo1Activities:" + repo1Activities.Count);
    var repo2Activities = DummyData.repo2Activities;
    Console.WriteLine("repo2Activities:" + repo2Activities.Count);

    // 저장소별 라벨 통계 요약 정보를 저장할 리스트
    var summaries = new List<(string RepoName, Dictionary<string, int> LabelCounts)>();

    // _client 초기화 
    RepoDataCollector.CreateClient(token);

    foreach (var repoPath in repos)
    {   
        // repoPath 파싱 및 형식 검사 
        var (owner,repo) = ParseRepoPath(repoPath);

        Console.WriteLine($"\n🔍 처리 중: {owner}/{repo}");
        try
        {
            // collector 생성
            var collector = new RepoDataCollector(owner, repo);

            // 데이터 수집
            var userActivities = collector.Collect();

            if (onlyPR)
            {
                // 이슈 활동 제거
                foreach (var activity in userActivities.Values)
                {
                    activity.IS_fb = 0;
                    activity.IS_doc = 0;
                }
            }
            else if (onlyIssue)
            {
                // PR 활동 제거
                foreach (var activity in userActivities.Values)
                {
                    activity.PR_fb = 0;
                    activity.PR_doc = 0;
                    activity.PR_typo = 0;
                }
            }

            // 테스트 출력, 라벨 카운트 기능 유지
            Dictionary<string, int> labelCounts = new Dictionary<string, int>
            {
                { "bug", 0 },
                { "documentation", 0 },
                { "typo", 0 }
            };
            string filePath = $"{repo}.txt";
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"=== {repo} Activities ===");
                foreach (var kvp in userActivities)
                {
                    string userId = kvp.Key;
                    UserActivity activity = kvp.Value;

                    writer.WriteLine($"User ID: {userId}");
                    writer.WriteLine($"  PR_fb: {activity.PR_fb}");
                    writer.WriteLine($"  PR_doc: {activity.PR_doc}");
                    writer.WriteLine($"  PR_typo: {activity.PR_typo}");
                    writer.WriteLine($"  IS_fb: {activity.IS_fb}");
                    writer.WriteLine($"  IS_doc: {activity.IS_doc}");
                    writer.WriteLine(); // 빈 줄

                    // 라벨 카운트
                    labelCounts["bug"] += activity.PR_fb + activity.IS_fb;
                    labelCounts["documentation"] += activity.PR_doc + activity.IS_doc;
                    labelCounts["typo"] += activity.PR_typo;
                }
            }
            summaries.Add(($"{owner}/{repo}", labelCounts));
        }
        catch (Exception e)
        {
            Console.WriteLine($"! 오류 발생: {e.Message}");
            continue;
        }

        try
        {
            var formats = format == null ?
                new List<string> { "text", "csv", "chart", "html" }
                : checkFormat(format);

            var outputDir = string.IsNullOrWhiteSpace(output) ? "output" : output;

            // 점수 계산 기능이 구현되지 않았으므로 현재 생성되는 파일은 모두 DummyData의 repo1Scores으로 만들어짐
            // 추후 계산 기능이 구현 후 반환되는 값을 DummyData.repo1Scores대신 전달해야합니다
            var generator = new FileGenerator(DummyData.repo1Scores, repo, outputDir);

            if (formats.Contains("csv"))
            {
                generator.GenerateCsv();
            }
            if (formats.Contains("text"))
            {
                generator.GenerateTable();
            }
            if (formats.Contains("chart"))
            {
                Console.WriteLine("차트 생성이 아직 구현되지 않았습니다.");
            }
            if (formats.Contains("html"))
            {
                Console.WriteLine("html 파일 생성이 아직 구현되지 않았습니다.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"! 오류 발생: {ex.Message}");
            continue;
        }
    }

    // 전체 저장소 요약 테이블 출력
    if (summaries.Count > 0)
    {
        Console.WriteLine("\n📊 전체 저장소 요약 통계");
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine($"{"Repo",-30} {"B/F",5} {"Doc",5} {"typo",5}");
        Console.WriteLine("----------------------------------------------------");

        foreach (var (repoName, counts) in summaries)
        {
            Console.WriteLine($"{repoName,-30} {counts["bug"],5} {counts["documentation"],5} {counts["typo"],5}");
        }
    }
});

static List<string> checkFormat(string[] format)
{
    var FormatList = new List<string> {"text", "csv", "chart", "html", "all"}; // 유효한 format

    var validFormats = new List<string> { };
    var unValidFormats = new List<string> { };
    char[] invalidChars = Path.GetInvalidFileNameChars();

    foreach (var fm in format)
    {
        var f = fm.Trim().ToLowerInvariant(); // 대소문자 구분 없이 유효성 검사
        if (f.IndexOfAny(invalidChars) >= 0)
        {
            Console.WriteLine($"포맷 '{f}'에는 파일명으로 사용할 수 없는 문자가 포함되어 있습니다.");
            Console.WriteLine("포맷 이름에서 다음 문자를 사용하지 마세요: " +
                string.Join(" ", invalidChars.Select(c => $"'{c}'")));
            Environment.Exit(1);
        }

        if (FormatList.Contains(f))
            validFormats.Add(f);
        else
            unValidFormats.Add(f);
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
        Environment.Exit(1);
    }

    // 추출한 리스트에 "all"이 존재할 경우 모든 포맷 리스트 반환
    if (validFormats.Contains("all"))
    {
        return new List<string> { "text", "csv", "chart", "html" };
    }

    return validFormats;
}

static (string, string) ParseRepoPath(string repoPath)
{
    var parts = repoPath.Split('/');
    if (parts.Length != 2)
    {
        Console.WriteLine($"! 저장소 인자 '{repoPath}'는 'owner/repo' 형식이어야 합니다.");
        Environment.Exit(1);
    }

    return (parts[0], parts[1]);
}