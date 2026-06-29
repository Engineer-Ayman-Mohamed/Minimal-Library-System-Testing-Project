using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Books;

public class BooksLoadTest : BooksLoadTestBase, IClassFixture<LoadTestBase>
{
    public BooksLoadTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }
    [Fact]
    public async Task GetBooks_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(
            LoadTestConfig.Load.Users,
            metrics,
            cts.Token,
            LoadTestConfig.Load.ThinkTimeMs);
        metrics.Stop();

        _output.WriteLine("=== LOAD TEST RESULTS ===");
        _output.WriteLine(metrics.Summary());

        AssertLoadThresholds(metrics);
    }
    private void AssertLoadThresholds(MetricsCollector metrics)
    {
        var failures = new List<string>();

        if (metrics.AverageMs > LoadTestConfig.Thresholds.MaxAverageMs)
            failures.Add($"Avg {metrics.AverageMs:F0}ms > {LoadTestConfig.Thresholds.MaxAverageMs}ms");

        if (metrics.Percentile(95) > LoadTestConfig.Thresholds.MaxP95Ms)
            failures.Add($"P95 {metrics.Percentile(95)}ms > {LoadTestConfig.Thresholds.MaxP95Ms}ms");

        if (metrics.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate)
            failures.Add($"Error rate {metrics.ErrorRate * 100:F2}% > {LoadTestConfig.Thresholds.MaxErrorRate * 100}%");

        Assert.False(failures.Any(), "Load thresholds violated:\n" + string.Join('\n', failures));
    }
}