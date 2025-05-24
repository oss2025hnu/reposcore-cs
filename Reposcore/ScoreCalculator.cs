public class ScoreCalculator
{
    public (int TotalScore, Dictionary<string, int> Breakdown) CalculateAll(UserActivity a)
    {
        // PR 유효 개수 계산
        int P_fb = a.PR_fb;
        int P_d = a.PR_doc;
        int P_t = a.PR_typo;
        int P_valid = P_fb + Math.Min(P_d + P_t, 3 * Math.Max(P_fb, 1));

        // PR 배분
        int P_fb_ = Math.Min(P_fb, P_valid);
        int P_d_ = Math.Min(P_d, P_valid - P_fb_);
        int P_t_ = Math.Max(P_valid - P_fb_ - P_d_, 0);

        // 이슈 유효 개수 계산
        int I_fb = a.IS_fb;
        int I_d = a.IS_doc;
        int I_valid = Math.Min(I_fb + I_d, 4 * P_valid);

        // 이슈 배분
        int I_fb_ = Math.Min(I_fb, I_valid);
        int I_d_ = Math.Max(I_valid - I_fb_, 0);

        // 최종 점수 계산
        int score = 3 * P_fb_ + 2 * P_d_ + 1 * P_t_ + 2 * I_fb_ + 1 * I_d_;

        var breakdown = new Dictionary<string, int>
        {
            ["PR_fb"] = P_fb_,
            ["PR_doc"] = P_d_,
            ["PR_typo"] = P_t_,
            ["IS_fb"] = I_fb_,
            ["IS_doc"] = I_d_
        };

        return (score, breakdown);
    }
}
