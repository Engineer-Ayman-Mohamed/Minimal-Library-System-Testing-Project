using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using LibrarySystem.LoadTests.Tests.LoadTests.Books;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.SpikeTests.Books;

public class BooksSpikeTest : BooksLoadTestBase, IClassFixture<LoadTestBase>
{
    public BooksSpikeTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task GetBooks_UnderSpikeLoad_RecoversThenNormal()
    {
        var normalBefore = new MetricsCollector();
        using var normalBeforeCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Spike.NormalDurationSeconds));

        _output.WriteLine("=== PHASE 1: Normal load ===");
        normalBefore.Start();
        await RunUsersAsync(
            LoadTestConfig.Spike.NormalUsers,
            normalBefore,
            normalBeforeCts.Token,
            LoadTestConfig.Spike.ThinkTimeMs);
        normalBefore.Stop();
        _output.WriteLine(normalBefore.Summary());

        var spike = new MetricsCollector();
        using var spikeCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Spike.SpikeDurationSeconds));

        _output.WriteLine("\n=== PHASE 2: Spike load ===");
        spike.Start();
        await RunUsersAsync(
            LoadTestConfig.Spike.SpikeUsers,
            spike,
            spikeCts.Token,
            LoadTestConfig.Spike.ThinkTimeMs);
        spike.Stop();
        _output.WriteLine(spike.Summary());
        
        var recovery = new MetricsCollector();
        using var recoveryCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Spike.RecoveryDurationSeconds));

        _output.WriteLine("\n=== PHASE 3: Recovery ===");
        recovery.Start();
        await RunUsersAsync(
            LoadTestConfig.Spike.NormalUsers,
            recovery,
            recoveryCts.Token,
            LoadTestConfig.Spike.ThinkTimeMs);
        recovery.Stop();
        _output.WriteLine(recovery.Summary());

        AssertSpikeThresholds(normalBefore, spike, recovery);
    }

    private void AssertSpikeThresholds(
        MetricsCollector normalBefore,
        MetricsCollector spike,
        MetricsCollector recovery)
    {
        var failures = new List<string>();

        if (normalBefore.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate)
            failures.Add($"[Phase 1] Error rate {normalBefore.ErrorRate * 100:F2}% " +
                         $"> {LoadTestConfig.Thresholds.MaxErrorRate * 100}%");

        if (normalBefore.AverageMs > LoadTestConfig.Thresholds.MaxAverageMs)
            failures.Add($"[Phase 1] Avg {normalBefore.AverageMs:F0}ms " +
                         $"> {LoadTestConfig.Thresholds.MaxAverageMs}ms");

        if (spike.ErrorRate > LoadTestConfig.Thresholds.MaxSpikeErrorRate)
            failures.Add($"[Phase 2] Spike error rate {spike.ErrorRate * 100:F2}% " +
                         $"> {LoadTestConfig.Thresholds.MaxSpikeErrorRate * 100}%");

        var recoveryP95Threshold =
            normalBefore.Percentile(95) * LoadTestConfig.Thresholds.MaxP95InRecoveryMultiplier;

        if (recovery.Percentile(95) > recoveryP95Threshold)
            failures.Add($"[Phase 3] Recovery P95 {recovery.Percentile(95)}ms " +
                         $"> {recoveryP95Threshold:F0}ms (1.5x baseline)");

        if (recovery.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate)
            failures.Add($"[Phase 3] Recovery error rate {recovery.ErrorRate * 100:F2}% " +
                         $"> {LoadTestConfig.Thresholds.MaxErrorRate * 100}%");

        _output.WriteLine("\n=== SPIKE TEST SUMMARY ===");
        _output.WriteLine($"Baseline  avg: {normalBefore.AverageMs:F0}ms | P95: {normalBefore.Percentile(95)}ms");
        _output.WriteLine($"Spike     avg: {spike.AverageMs:F0}ms | P95: {spike.Percentile(95)}ms | errors: {spike.ErrorRate * 100:F2}%");
        _output.WriteLine($"Recovery  avg: {recovery.AverageMs:F0}ms | P95: {recovery.Percentile(95)}ms | errors: {recovery.ErrorRate * 100:F2}%");

        Assert.False(failures.Any(), "Spike thresholds violated:\n" + string.Join('\n', failures));
    }
}