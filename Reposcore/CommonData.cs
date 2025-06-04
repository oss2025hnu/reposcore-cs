using System.Collections.Generic;

// 가변 속성으로 바꾼 UserActivity 레코드
public record UserActivity
{
    public int PR_fb { get; set; }
    public int PR_doc { get; set; }
    public int PR_typo { get; set; }
    public int IS_fb { get; set; }
    public int IS_doc { get; set; }
}


    // UserActivity를 분석해서 사용자별 점수를 계산하는 레코드
    public record UserScore(
        int PR_fb,
        int PR_doc,
        int PR_typo,
        int IS_fb,
        int IS_doc,
        int total
    );

// 1번 단계를 책임지는 Repscore/RepoDataCollector.cs의 클래스의 객체 하나가
// 모아오는 데이타가 바로 repo1Activities 같은 것이다.
public static class DummyData
{
public static Dictionary<string, UserActivity> repo1Activities = new() {
    { "user00", new UserActivity { PR_fb = 1, PR_doc = 0, PR_typo = 0, IS_fb = 0, IS_doc = 0 } },
    { "user01", new UserActivity { PR_fb = 0, PR_doc = 1, PR_typo = 0, IS_fb = 0, IS_doc = 0 } },
    { "user02", new UserActivity { PR_fb = 0, PR_doc = 0, PR_typo = 1, IS_fb = 0, IS_doc = 0 } },
    { "user03", new UserActivity { PR_fb = 0, PR_doc = 0, PR_typo = 0, IS_fb = 1, IS_doc = 0 } },
    { "user04", new UserActivity { PR_fb = 0, PR_doc = 0, PR_typo = 0, IS_fb = 0, IS_doc = 1 } },
    { "user05", new UserActivity { PR_fb = 10, PR_doc = 0, PR_typo = 0, IS_fb = 0, IS_doc = 0 } },
    { "user06", new UserActivity { PR_fb = 0, PR_doc = 10, PR_typo = 0, IS_fb = 0, IS_doc = 0 } },
    { "user07", new UserActivity { PR_fb = 0, PR_doc = 0, PR_typo = 10, IS_fb = 0, IS_doc = 0 } },
    { "user08", new UserActivity { PR_fb = 0, PR_doc = 0, PR_typo = 0, IS_fb = 10, IS_doc = 0 } },
    { "user09", new UserActivity { PR_fb = 0, PR_doc = 0, PR_typo = 0, IS_fb = 0, IS_doc = 10 } },
};

public static Dictionary<string, UserActivity> repo2Activities = new() {
    { "user03", new UserActivity { PR_fb = 26, PR_doc = 27, PR_typo = 28, IS_fb = 29, IS_doc = 30 } },
    { "user04", new UserActivity { PR_fb = 31, PR_doc = 32, PR_typo = 33, IS_fb = 34, IS_doc = 35 } },
    { "user05", new UserActivity { PR_fb = 36, PR_doc = 37, PR_typo = 38, IS_fb = 39, IS_doc = 40 } },
    { "user06", new UserActivity { PR_fb = 41, PR_doc = 42, PR_typo = 43, IS_fb = 44, IS_doc = 45 } },
    { "user08", new UserActivity { PR_fb = 12, PR_doc = 5, PR_typo = 8, IS_fb = 3, IS_doc = 17 } },
    { "user09", new UserActivity { PR_fb = 7, PR_doc = 14, PR_typo = 2, IS_fb = 19, IS_doc = 6 } },
    { "user10", new UserActivity { PR_fb = 21, PR_doc = 9, PR_typo = 13, IS_fb = 4, IS_doc = 11 } },
    { "user11", new UserActivity { PR_fb = 2, PR_doc = 18, PR_typo = 7, IS_fb = 15, IS_doc = 10 } },
    { "user12", new UserActivity { PR_fb = 16, PR_doc = 3, PR_typo = 12, IS_fb = 8, IS_doc = 14 } },
};


    public static Dictionary<string, UserScore> repo1Scores = new()
    {
        {"user01", new UserScore(21, 8, 0, 4, 3, 36)},
        {"user02", new UserScore(12, 6, 5, 2, 1, 26)},
        {"user03", new UserScore(3, 2, 3, 6, 2, 16)},
        {"user04", new UserScore(18, 10, 4, 8, 1, 41)},
        {"user05", new UserScore(9, 4, 2, 2, 5, 22)},
        {"user06", new UserScore(6, 12, 1, 6, 3, 28)},
        {"user07", new UserScore(15, 14, 5, 4, 2, 40)},
        {"user08", new UserScore(27, 16, 3, 10, 4, 60)},
        {"user09", new UserScore(30, 6, 0, 12, 1, 49)},
        {"user10", new UserScore(24, 18, 2, 14, 2, 60)},
        {"user11", new UserScore(33, 20, 4, 16, 5, 78)}
    };
}


/*
1단계 RepoDataCollector

하나의 저장소를 담당하는 객체 하나마다
Dictionary<String, UserActivity> 값 1개씩 만들어냄
          사용자이름  활동건수

Dictionary<String, UserActivity> 이거가 이름이 기니까
이거를 줄인 이름을 만드는 것도???
          
2단계 ***Analyzer

이것도 꼭 여러 개를 분석한다는 개념이 아니라
그냥 하나의 정보를 분석한다고 치고
Dictionary<String, UserActivity> 이거 한 개 넘겨받아서 분석하고
여러 번 각 저장소마다 분석하고

파이썬이나 JS쪽처럼 대략 하면 될 듯
 
어떤 데이터 구조를 생성해야 하냐? 대략 이름이 UserScore 같은
점수 구성요소들을 정라하는 레코드/구조체 타입을 정의해야 함 

여기 단계에서 하는 일을 데이터 구조 중심으로 정리하자면
사용자별 UserActivity를 보고 사용자별 UserScore를 만들어냄

그러니까 ***Analyzer객체가 만들어내는 데이터는 대략
Dictionary<String, UserScore> 가 된다는 말

 
3단계 ....
....
*/
