using System.Collections.Generic;

// 깃헙에서 우리가 필요한 정보를 가져와서 담아놓는 레코드 (각 내역별 활동 횟수)
public record UserActivity( // 나중에 적당한 다른 곳으로 옮겨야 함
    int PR_fb,
    int PR_doc,
    int PR_typo,
    int IS_fb,
    int IS_doc
);

// 1번 단계를 책임지는 Repscore/RepoDataCollector.cs의 클래스의 객채 하나가
// 모아오는 데이타가 바로 repo1Activities 같은 것이다.
public static class DummyData {
    public static Dictionary<string, UserActivity> repo1Activities = new() {
        { "user00", new UserActivity(1, 0, 0, 0, 0) },
        { "user01", new UserActivity(0, 1, 0, 0, 0) },
        { "user02", new UserActivity(0, 0, 1, 0, 0) },
        { "user03", new UserActivity(0, 0, 0, 1, 0) },
        { "user04", new UserActivity(0, 0, 0, 0, 1) },
        { "user05", new UserActivity(10, 0, 0, 0, 0) },
        { "user06", new UserActivity(0, 10, 0, 0, 0) },
        { "user07", new UserActivity(0, 0, 10, 0, 0) },
        { "user08", new UserActivity(0, 0, 0, 10, 0) },
        { "user09", new UserActivity(0, 0, 0, 0, 10) },
    };
    public static Dictionary<string, UserActivity> repo2Activities = new() {
        { "user03", new UserActivity(26, 27, 28, 29, 30) },
        { "user04", new UserActivity(31, 32, 33, 34, 35) },
        { "user05", new UserActivity(36, 37, 38, 39, 40) },
        { "user06", new UserActivity(41, 42, 43, 44, 45) },
        { "user08", new UserActivity(12, 5, 8, 3, 17) },
        { "user09", new UserActivity(7, 14, 2, 19, 6) },
        { "user10", new UserActivity(21, 9, 13, 4, 11) },
        { "user11", new UserActivity(2, 18, 7, 15, 10) },
        { "user12", new UserActivity(16, 3, 12, 8, 14) },
    };
}
