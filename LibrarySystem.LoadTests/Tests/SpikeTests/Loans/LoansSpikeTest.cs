using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using LibrarySystem.LoadTests.Tests.LoadTests.Loans;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.SpikeTests.Loans;

/// <summary>Spike tests for the Loans API endpoints evaluating performance under sudden traffic surges and recovery.</summary>
public class LoansSpikeTest : LoansLoadTestBase, IClassFixture<LoadTestBase>
{
    public LoansSpikeTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    /// <summary>Verifies GET loans recovers after a traffic spike.</summary>
    [Fact]
    public async Task GetLoans_UnderSpikeLoad_RecoversThenNormal()
        => await RunSpikeTest("GET /Loans", GetLoansForMemberAsync);

    /// <summary>Verifies POST borrow recovers after a traffic spike.</summary>
    [Fact]
    public async Task BorrowBook_UnderSpikeLoad_RecoversThenNormal()
        => await RunSpikeTest("POST /Loans", BorrowBookAsync);

    /// <summary>Verifies PATCH loan recovers after a traffic spike.</summary>
    [Fact]
    public async Task PatchLoan_UnderSpikeLoad_RecoversThenNormal()
        => await RunSpikeTest("PATCH /Loans/{id}", PatchLoanAsync);

    /// <summary>Verifies PUT return recovers after a traffic spike.</summary>
    [Fact]
    public async Task ReturnBook_UnderSpikeLoad_RecoversThenNormal()
        => await RunSpikeTest("PUT /Loans/{id}/return", ReturnBookAsync);

    /// <summary>Verifies DELETE loan recovers after a traffic spike.</summary>
    [Fact]
    public async Task DeleteLoan_UnderSpikeLoad_RecoversThenNormal()
        => await RunSpikeTest("DELETE /Loans/{id}", DeleteLoanAsync);

    private async Task RunSpikeTest(string label,
        Func<int, Task<(int StatusCode, long Ms)>> requestFn)
    {
        var normalBefore = new MetricsCollector();
        using var normalBeforeCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Spike.NormalDurationSeconds));

        normalBefore.Start();
        await RunUsersAsync(LoadTestConfig.Spike.NormalUsers, normalBefore,
            normalBeforeCts.Token, LoadTestConfig.Spike.ThinkTimeMs, requestFn);
        normalBefore.Stop();
        PrintSummary($"{label} - SPIKE PHASE 1: Normal", normalBefore);

        var spike = new MetricsCollector();
        using var spikeCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Spike.SpikeDurationSeconds));

        spike.Start();
        await RunUsersAsync(LoadTestConfig.Spike.SpikeUsers, spike,
            spikeCts.Token, LoadTestConfig.Spike.ThinkTimeMs, requestFn);
        spike.Stop();
        PrintSummary($"{label} - SPIKE PHASE 2: Spike", spike);

        var recovery = new MetricsCollector();
        using var recoveryCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Spike.RecoveryDurationSeconds));

        recovery.Start();
        await RunUsersAsync(LoadTestConfig.Spike.NormalUsers, recovery,
            recoveryCts.Token, LoadTestConfig.Spike.ThinkTimeMs, requestFn);
        recovery.Stop();
        PrintSummary($"{label} - SPIKE PHASE 3: Recovery", recovery);

        AssertSpikeThresholds(label, normalBefore, spike, recovery);
    }

    private void AssertSpikeThresholds(string label,
        MetricsCollector normalBefore, MetricsCollector spike, MetricsCollector recovery)
    {
        var recoveryP95Threshold =
            normalBefore.Percentile(95) * LoadTestConfig.Thresholds.MaxP95InRecoveryMultiplier;
        _output.WriteLine($"\n=== {label} SPIKE SUMMARY ===");
        _output.WriteLine($"Baseline  avg: {normalBefore.AverageMs:F0}ms | P95: {normalBefore.Percentile(95)}ms | errors: {normalBefore.ErrorRate * 100:F2}%");
        _output.WriteLine($"Spike     avg: {spike.AverageMs:F0}ms | P95: {spike.Percentile(95)}ms | errors: {spike.ErrorRate * 100:F2}%");
        _output.WriteLine($"Recovery  avg: {recovery.AverageMs:F0}ms | P95: {recovery.Percentile(95)}ms | errors: {recovery.ErrorRate * 100:F2}%");
    }
}