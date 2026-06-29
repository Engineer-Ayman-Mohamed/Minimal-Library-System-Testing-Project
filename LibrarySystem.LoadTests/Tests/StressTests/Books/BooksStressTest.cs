using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using LibrarySystem.LoadTests.Tests.LoadTests.Books;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.StressTests.Books;

public class BooksStressTest : BooksLoadTestBase, IClassFixture<LoadTestBase>
{
    public BooksStressTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task GetBooks_UnderIncreasingLoad_FindsBreakingPoint()
    {
        var stageResults = new List<(int Users, MetricsCollector Metrics)>();

        foreach (var userCount in LoadTestConfig.Stress.UserStages)
        {
            var metrics = new MetricsCollector();
            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(LoadTestConfig.Stress.StageDurationSeconds));

            _output.WriteLine($"\n=== STRESS STAGE: {userCount} users ===");

            metrics.Start();
            await RunUsersAsync(
                userCount,
                metrics,
                cts.Token,
                LoadTestConfig.Stress.ThinkTimeMs);
            metrics.Stop();

            _output.WriteLine(metrics.Summary());
            stageResults.Add((userCount, metrics));

            if (metrics.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate)
            {
                _output.WriteLine($">>> Breaking point reached at {userCount} users");
                _output.WriteLine($">>> Error rate: {metrics.ErrorRate * 100:F2}%");
                break;
            }
        }

        AssertStressThresholds(stageResults);
    }

    private void AssertStressThresholds(List<(int Users, MetricsCollector Metrics)> stageResults)
    {
        var firstStage = stageResults.First();
        var failures = new List<string>();

        if (firstStage.Metrics.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate)
            failures.Add($"Even at {firstStage.Users} users error rate " +
                         $"{firstStage.Metrics.ErrorRate * 100:F2}% exceeded threshold");

        if (firstStage.Metrics.AverageMs > LoadTestConfig.Thresholds.MaxAverageMs)
            failures.Add($"Even at {firstStage.Users} users avg response " +
                         $"{firstStage.Metrics.AverageMs:F0}ms exceeded threshold");

        var breakingStage = stageResults
            .FirstOrDefault(s => s.Metrics.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate);

        if (breakingStage != default)
            _output.WriteLine($"\n>>> System degraded at {breakingStage.Users} users");
        else
            _output.WriteLine($"\n>>> System held through all {stageResults.Last().Users} users");

        Assert.False(failures.Any(), "Stress baseline failed:\n" + string.Join('\n', failures));
    }
}