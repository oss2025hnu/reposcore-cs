using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetEnv;

// GitHub 저장소 데이터를 수집하는 클래스입니다.
// 저장소의 PR 및 이슈 데이터를 분석하고, 사용자별 활동 정보를 정리합니다.
/// <summary>
/// GitHub 저장소에서 이슈 및 PR 데이터를 수집하고 사용자별 활동 내역을 생성하는 클래스입니다.
/// </summary>
/// <remarks>
/// 이 클래스는 Octokit 라이브러리를 사용하여 GitHub API로부터 데이터를 가져오며,
/// 사용자 활동을 분석해 <see cref="UserActivity"/> 형태로 정리합니다.
/// </remarks>
/// <param name="owner">GitHub 저장소 소유자 (예: oss2025hnu)</param>
/// <param name="repo">GitHub 저장소 이름 (예: reposcore-cs)</param>
public class RepoDataCollector
{
    private static GitHubClient? _client; // GitHub API 요청에 사용할 클라이언트입니다.
    private readonly string _owner; // 분석 대상 저장소의 owner (예: oss2025hnu)
    private readonly string _repo; // 분석 대상 저장소의 이름 (예: reposcore-cs)
    private readonly bool _showApiLimit;

     //수정에 용이하도록 수집데이터종류 전역변수화
    private static readonly string[] FeatureLabels = { "bug", "enhancement" };
    private static readonly string[] DocsLabels = { "documentation" };
    private static readonly string TypoLabel = "typo";

    // 생성자에는 저장소 하나의 정보를 넘김
    public RepoDataCollector(string owner, string repo, bool showApiLimit = false)
    {
        _owner = owner;
        _repo = repo;
        _showApiLimit = showApiLimit;
    }
    
    // GitHubClient 초기화 메소드
    public static void CreateClient(string? token = null)
    {
        _client = new GitHubClient(new ProductHeaderValue("reposcore-cs"));

        // 인증키 추가 (토큰이 있을경우)
        // 토큰이 직접 전달된 경우: .env 갱신 후 인증 설정
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
                    if (ex.Message.Contains("Bad credentials"))
                        Console.WriteLine("⚠️ 인증 토큰이 잘못되었습니다. 새로운 토큰을 발급받아 사용하세요.");
                    else
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
                        if (ex.Message.Contains("Bad credentials"))
                            Console.WriteLine("⚠️ 인증 토큰이 잘못되었습니다. 새로운 토큰을 발급받아 사용하세요.");
                        else
                            Console.WriteLine($"⚠️ RateLimit 정보 조회 실패: {ex.Message}");
                    }
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
                    if (ex.Message.Contains("Bad credentials"))
                        Console.WriteLine("⚠️ 인증 토큰이 잘못되었습니다. 새로운 토큰을 발급받아 사용하세요.");
                    else
                        Console.WriteLine($"⚠️ RateLimit 정보 조회 실패 (종료 후): {ex.Message}");
                }
            }
            // 레코드로 변환
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
                if (innerEx.Message.Contains("Bad credentials"))
                    Console.WriteLine("⚠️ 인증 토큰이 잘못되었습니다. 새로운 토큰을 발급받아 사용하세요.");
                else
                    Console.WriteLine($"❗[{_owner}/{_repo}] 한도 초과 상태 조회 실패: {innerEx.Message}");
            }

            Environment.Exit(1);
        }
        catch (AuthorizationException)
        {
            Console.WriteLine($"❗[{_owner}/{_repo}] 인증 실패: 올바른 토큰을 사용했는지 확인하세요.");
            Environment.Exit(1);
        }
        catch (NotFoundException)
        {
            Console.WriteLine($"❗[{_owner}/{_repo}] 저장소를 찾을 수 없습니다. owner/repo 이름을 확인하세요.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Bad credentials"))
                Console.WriteLine("⚠️ 인증 토큰이 잘못되었습니다. 새로운 토큰을 발급받아 사용하세요.");
            else
                Console.WriteLine($"❗[{_owner}/{_repo}] 알 수 없는 오류: {ex.Message}");
            Environment.Exit(1);
        }

        return null!;
    }
}
