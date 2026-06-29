using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Loans;

public class LoansLoadTest : LoansLoadTestBase, IClassFixture<LoadTestBase>
{
    public LoansLoadTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task GetLoans_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, GetLoansForMemberAsync);
        metrics.Stop();

        PrintSummary("GET LOANS - LOAD TEST", metrics);
        AssertLoadThresholds(metrics, "GET /Loans");
    }

    [Fact]
    public async Task BorrowBook_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, BorrowBookAsync);
        metrics.Stop();

        PrintSummary("POST LOANS (BORROW) - LOAD TEST", metrics);
        AssertLoadThresholds(metrics, "POST /Loans");
    }

    [Fact]
    public async Task PatchLoan_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, PatchLoanAsync);
        metrics.Stop();

        PrintSummary("PATCH LOANS - LOAD TEST", metrics);
        AssertLoadThresholds(metrics, "PATCH /Loans/{id}");
    }

    [Fact]
    public async Task ReturnBook_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, ReturnBookAsync);
        metrics.Stop();

        PrintSummary("PUT LOANS/RETURN - LOAD TEST", metrics);
        AssertLoadThresholds(metrics, "PUT /Loans/{id}/return");
    }

    [Fact]
    public async Task DeleteLoan_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, DeleteLoanAsync);
        metrics.Stop();

        PrintSummary("DELETE LOANS - LOAD TEST", metrics);
        AssertLoadThresholds(metrics, "DELETE /Loans/{id}");
    }
}