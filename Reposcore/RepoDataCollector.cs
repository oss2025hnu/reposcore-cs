using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetEnv;

public class RepoDataCollector
{
    private static GitHubClient? _client;
    private readonly string _owner;
    private readonly string _repo;
    private readonly bool _showApiLimit; // 

    private static readonly string[] FeatureLabels = { "bug", "enhancement" };
    private static readonly string[] DocsLabels = { "documentation" };
    private static readonly string TypoLabel = "typo";

    // 생성자 수정: showApiLimit 매개변수 추가
    public RepoDataCollector(string owner, string repo, bool showApiLimit = false)
    {
        _owner = owner;
        _repo = repo;
        _showApiLimit = showApiLimit;
    }

    public static void CreateClient(string? token = null)
    {
        _client = new GitHubClient(new ProductHeaderValue("reposcore-cs"));

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                File.WriteAllText(".env", $"GITHUB_TOKEN={token}\n");
                Console.WriteLine(".env의 토큰을 갱신합니다.");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"❗ .env 파일 쓰기 중 IO 오류가 발생했습니다: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"❗ .env 파일 쓰기 권한이 없습니다: {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ .env 파일 쓰기 중 알 수 없는 오류가 발생했습니다: {ex.Message}");
            }

            _client.Credentials = new Credentials(token);
        }
        else if (File.Exists(".env"))
        {
            try
            {
                Console.WriteLine(".env의 토큰으로 인증을 진행합니다.");
                Env.Load();
                token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("❗ .env 파일에는 GITHUB_TOKEN이 포함되어 있지 않습니다.");
                }
                else
                {
                    _client.Credentials = new Credentials(token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ .env 파일 로딩 중 오류가 발생했습니다: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("❗ 인증 토큰이 제공되지 않았고 .env 파일도 존재하지 않습니다. 인증이 실패할 수 있습니다.");
        }
    }

    public Dictionary<string, UserActivity> Collect(bool returnDummyData = false, string? since = null, string? until = null)
    {
        if (returnDummyData)
        {
            return DummyData.repo1Activities;
        }

        try
        {
            var request = new RepositoryIssueRequest
            {
                State = ItemStateFilter.All
            };

            if (!string.IsNullOrEmpty(since))
            {
                if (DateTime.TryParse(since, out DateTime sinceDate))
                {
                    request.Since = sinceDate;
                }
                else
                {
                    throw new ArgumentException($"잘못된 시작 날짜 형식입니다: {since}. YYYY-MM-DD 형식으로 입력해주세요.");
                }
            }

            // API 한도 정보 시작 시 출력
           if (_showApiLimit)
{
    try
    {
        var rate = _client?.RateLimit.GetRateLimits().Result.Rate;
        if (rate == null)
            Console.WriteLine("⚠️ 인증되지 않아 RateLimit 정보를 출력할 수 없습니다.");
        else
            Console.WriteLine($"🚀 [{_owner}/{_repo}] 분석 시작 전 RateLimit: Remaining={rate.Remaining}, Reset={rate.Reset.LocalDateTime}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ RateLimit 정보 조회 실패 (시작 전): {ex.Message}");
    }
}


            var allIssuesAndPRs = _client!.Issue.GetAllForRepository(_owner, _repo, request).Result;

            if (!string.IsNullOrEmpty(until))
            {
                if (!DateTime.TryParse(until, out DateTime untilDate))
                {
                    throw new ArgumentException($"잘못된 종료 날짜 형식입니다: {until}. YYYY-MM-DD 형식으로 입력해주세요.");
                }
                allIssuesAndPRs = allIssuesAndPRs.Where(issue => issue.CreatedAt <= untilDate).ToList();
            }

            var mutableActivities = new Dictionary<string, UserActivity>();
            int count = 0; 

            foreach (var item in allIssuesAndPRs)
            {
                if (item.User?.Login == null) continue;

                var username = item.User.Login;

                if (!mutableActivities.ContainsKey(username))
                {
                    mutableActivities[username] = new UserActivity(0, 0, 0, 0, 0);
                }

                var labelName = item.Labels.Any() ? item.Labels[0].Name : null;
                var activity = mutableActivities[username];

                if (item.PullRequest != null)
                {
                    if (item.PullRequest.Merged)
                    {
                        if (FeatureLabels.Contains(labelName))
                            activity.PR_fb++;
                        else if (DocsLabels.Contains(labelName))
                            activity.PR_doc++;
                        else if (labelName == TypoLabel)
                            activity.PR_typo++;
                    }
                }
                else
                {
                    if (item.State.Value.ToString() == "Open" ||
                        item.StateReason.ToString() == "completed")
                    {
                        if (FeatureLabels.Contains(labelName))
                            activity.IS_fb++;
                        else if (DocsLabels.Contains(labelName))
                            activity.IS_doc++;
                    }
                }

                count++;

                // 20개마다 호출 한도 출력
                if (_showApiLimit && count % 20 == 0)
{
    try
    {
        var rate = _client?.RateLimit.GetRateLimits().Result.Rate;
        if (rate == null)
            Console.WriteLine("⚠️ 인증되지 않아 RateLimit 정보를 출력할 수 없습니다.");
        else
            Console.WriteLine($"📡 [RateLimit] Remaining={rate.Remaining}, Reset={rate.Reset.LocalDateTime}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ RateLimit 정보 조회 실패: {ex.Message}");
    }
}


            // API 한도 정보 종료 시 출력
           if (_showApiLimit)
{
    try
    {
        var rate = _client?.RateLimit.GetRateLimits().Result.Rate;
        if (rate == null)
            Console.WriteLine("⚠️ 인증되지 않아 RateLimit 정보를 출력할 수 없습니다.");
        else
            Console.WriteLine($"✅ [{_owner}/{_repo}] 분석 종료 후 RateLimit: Remaining={rate.Remaining}, Reset={rate.Reset.LocalDateTime}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ RateLimit 정보 조회 실패 (종료 후): {ex.Message}");
    }
}


            var userActivities = new Dictionary<string, UserActivity>();
            foreach (var (key, value) in mutableActivities)
            {
                userActivities[key] = new UserActivity(
                    PR_fb: value.PR_fb,
                    PR_doc: value.PR_doc,
                    PR_typo: value.PR_typo,
                    IS_fb: value.IS_fb,
                    IS_doc: value.IS_doc
                );
            }

            return userActivities;
        }
        catch (RateLimitExceededException)
        {
            try
            {
                var rateLimits = _client!.RateLimit.GetRateLimits().Result;
                var coreRateLimit = rateLimits.Rate;
                var resetTime = coreRateLimit.Reset;
                var secondsUntilReset = (int)(resetTime - DateTimeOffset.UtcNow).TotalSeconds;

                Console.WriteLine($"❗[{_owner}/{_repo}] API 호출 한도 초과. {secondsUntilReset}초 후 재시도 가능 (약 {resetTime.LocalDateTime})");
            }
            catch (Exception innerEx)
            {
                Console.WriteLine($"❗[{_owner}/{_repo}] 한도 초과 상태 조회 실패: {innerEx.Message}");
            }

            Environment.Exit(1);
        }
        catch (AuthorizationException)
        {
            Console.WriteLine("❗[{_owner}/{_repo}] 인증 실패: 올바른 토큰을 사용했는지 확인하세요.");
            Environment.Exit(1);
        }
        catch (NotFoundException)
        {
            Console.WriteLine("❗[{_owner}/{_repo}] 저장소를 찾을 수 없습니다. owner/repo 이름을 확인하세요.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❗[{_owner}/{_repo}] 알 수 없는 오류: {ex.Message}");
            Environment.Exit(1);
        }

        return null!;
    }
}
