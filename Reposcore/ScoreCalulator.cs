namespace Reposcore
{
    public record ScoreComponents(
        int FeaturePR,
        int BugfixPR,
        int DocsPR,
        int TypoPR,
        int FeatureIssue,
        int DocsIssue
    );
}

namespace Reposcore
{
    public static class ScoreCalculator
    {
        public static int CalculateTotalScore(ScoreComponents components)
        {
            int score = 0;

            score += components.FeaturePR * 3;
            score += components.BugfixPR * 3;
            score += components.DocsPR * 2;
            score += components.TypoPR * 1;
            score += components.FeatureIssue * 2;
            score += components.DocsIssue * 1;

            return score;
        }

        public static int CalculateValidPRCount(ScoreComponents components)
        {
            int contentPRs = components.FeaturePR + components.BugfixPR;
            int supportPRs = components.DocsPR + components.TypoPR;

            return components.FeaturePR + Math.Min(supportPRs, 3 * Math.Max(components.FeaturePR, 1));
        }

        public static int CalculateValidIssueCount(ScoreComponents components)
        {
            int validPR = CalculateValidPRCount(components);
            int issueSum = components.FeatureIssue + components.DocsIssue;

            return Math.Min(issueSum, 4 * validPR);
        }
    }
}
