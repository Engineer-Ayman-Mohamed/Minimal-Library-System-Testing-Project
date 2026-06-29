using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using LibrarySystem.LoadTests.Tests.LoadTests.Loans;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.StressTests.Loans;

/// <summary>Stress tests for the Loans API endpoints finding the breaking point under increasing concurrent user load.</summary>
public class LoansStressTest : LoansLoadTestBase, IClassFixture<LoadTestBase>
{
    public LoansStressTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    /// <summary>Finds the breaking point for GET loans under staged user load.</summary>
    [Fact]
    public async Task GetLoans_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("GET /Loans", GetLoansForMemberAsync);

    /// <summary>Finds the breaking point for POST borrow under staged user load.</summary>
    [Fact]
    public async Task BorrowBook_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("POST /Loans", BorrowBookAsync);

    /// <summary>Finds the breaking point for PATCH loan under staged user load.</summary>
    [Fact]
    public async Task PatchLoan_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("PATCH /Loans/{id}", PatchLoanAsync);

    /// <summary>Finds the breaking point for PUT return under staged user load.</summary>
    [Fact]
    public async Task ReturnBook_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("PUT /Loans/{id}/return", ReturnBookAsync);

    /// <summary>Finds the breaking point for DELETE loan under staged user load.</summary>
    [Fact]
    public async Task DeleteLoan_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("DELETE /Loans/{id}", DeleteLoanAsync);

    private async Task RunStressTest(
        string label,
        Func<int, Task<(int StatusCode, long Ms)>> requestFn
    ) {
        var stageResults = new List<(int Users, MetricsCollector Metrics)>();

        foreach (var userCount in LoadTestConfig.Stress.UserStages)
        {
            var metrics = new MetricsCollector();
            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(LoadTestConfig.Stress.StageDurationSeconds));

            metrics.Start();
            await RunUsersAsync(userCount, metrics, cts.Token,
                LoadTestConfig.Stress.ThinkTimeMs, requestFn);
            metrics.Stop();

            PrintSummary($"{label} - STRESS STAGE {userCount} USERS", metrics);
            stageResults.Add((userCount, metrics));

            if (metrics.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate)
            {
                _output.WriteLine($">>> Breaking point reached at {userCount} users");
                break;
            }
        }

        AssertStressThresholds(label, stageResults);
    }

    private void AssertStressThresholds(string label,
        List<(int Users, MetricsCollector Metrics)> stageResults)
    {
        var first = stageResults.First();

        var breakingStage = stageResults
            .FirstOrDefault(s => s.Metrics.ErrorRate > LoadTestConfig.Thresholds.MaxErrorRate);

        _output.WriteLine(breakingStage != default
            ? $"\n>>> [{label}] Breaking point: {breakingStage.Users} users"
            : $"\n>>> [{label}] Held through all {stageResults.Last().Users} users");
    }
}