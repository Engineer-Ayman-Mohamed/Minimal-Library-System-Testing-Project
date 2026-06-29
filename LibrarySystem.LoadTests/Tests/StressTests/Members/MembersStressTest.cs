using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using LibrarySystem.LoadTests.Tests.LoadTests.Members;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.StressTests.Members;

/// <summary>Stress tests for the Members API endpoints finding the breaking point under increasing concurrent user load.</summary>
public class MembersStressTest : MembersLoadTestBase, IClassFixture<LoadTestBase>
{
    public MembersStressTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    /// <summary>Finds the breaking point for GET member under staged user load.</summary>
    [Fact]
    public async Task GetMemberById_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("GET /Members/{id}", GetMemberByIdAsync);

    /// <summary>Finds the breaking point for POST member under staged user load.</summary>
    [Fact]
    public async Task CreateMember_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("POST /Members", CreateMemberAsync);

    /// <summary>Finds the breaking point for PATCH member under staged user load.</summary>
    [Fact]
    public async Task UpdateMember_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("PATCH /Members/{id}", UpdateMemberAsync);

    /// <summary>Finds the breaking point for DELETE member under staged user load.</summary>
    [Fact]
    public async Task DeleteMember_UnderIncreasingLoad_FindsBreakingPoint()
        => await RunStressTest("DELETE /Members/{id}", DeleteMemberAsync);

    private async Task RunStressTest(string label,
        Func<int, Task<(int StatusCode, long Ms)>> requestFn)
    {
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
                _output.WriteLine($">>> Error rate: {metrics.ErrorRate * 100:F2}%");
                break;
            }
        }
    }
    
}