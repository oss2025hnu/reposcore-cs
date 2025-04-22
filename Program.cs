using Cocona;
using System;
using System.Collections.Generic;

class Program
{
    public static void Main(string[] args)
    {
        CoconaApp.Run<Program>(args);
    }

    public void Run(
        [Argument] string[] repository,
        [Option('v')] bool verbose = false,
        [Option("labels")] string labelConfigPath = "labels.json"
    )
    {
        Console.WriteLine($"Repositories: {string.Join(", ", repository)}");

        if (verbose)
        {
            Console.WriteLine("Verbose mode ON");
        }

        // 1. 라벨 설정 읽기
        var labelConfig = LabelLoader.LoadLabelConfig(labelConfigPath);

        // 2. 샘플 라벨들 (이 부분은 실제 PR이나 이슈 분석 코드에서 대체)
        var sampleLabels = new[] { "bug", "refactor", "style", "invalid" };

        // 3. 중복 제거된 라벨 카운팅
        var labelCounts = new Dictionary<string, int>();
        foreach (var label in sampleLabels)
        {
            CountLabel(label, labelCounts, labelConfig.FeatureLabels);
        }

        // 4. 출력
        Console.WriteLine("=== Label Counts ===");
        foreach (var kvp in labelCounts)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }

    private static void CountLabel(string labelName, Dictionary<string, int> counts, List<string> validLabels)
    {
        string lower = labelName.ToLower();

        if (!validLabels.Contains(lower)) return;

        if (!counts.ContainsKey(lower))
            counts[lower] = 0;

        counts[lower]++;
    }
}
