using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Loans;

/// <summary>Load tests for the Loans API endpoints (get, borrow, patch, return, delete) under normal concurrent user load.</summary>
public class LoansLoadTest : LoansLoadTestBase, IClassFixture<LoadTestBase>
{
    public LoansLoadTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    /// <summary>Verifies that GET loans under normal load stays within response time and error rate thresholds.</summary>
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

    /// <summary>Verifies that POST borrow book under normal load stays within performance thresholds.</summary>
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

    /// <summary>Verifies that PATCH loan under normal load stays within performance thresholds.</summary>
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

    /// <summary>Verifies that PUT return book under normal load stays within performance thresholds.</summary>
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

    /// <summary>Verifies that DELETE loan under normal load stays within performance thresholds.</summary>
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