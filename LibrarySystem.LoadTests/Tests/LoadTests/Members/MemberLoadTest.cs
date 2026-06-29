using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Members;

/// <summary>Load tests for the Members API endpoints under normal concurrent user load.</summary>
public class MembersLoadTest : MembersLoadTestBase, IClassFixture<LoadTestBase>
{
    public MembersLoadTest(LoadTestBase fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    /// <summary>Verifies that GET member by ID under normal load stays within performance thresholds.</summary>
    [Fact]
    public async Task GetMemberById_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, GetMemberByIdAsync);
        metrics.Stop();

        PrintSummary("GET MEMBER BY ID - LOAD TEST", metrics);
    }

    /// <summary>Verifies that POST member under normal load stays within performance thresholds.</summary>
    [Fact]
    public async Task CreateMember_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, CreateMemberAsync);
        metrics.Stop();

        PrintSummary("POST MEMBERS - LOAD TEST", metrics);
    }

    /// <summary>Verifies that PATCH member under normal load stays within performance thresholds.</summary>
    [Fact]
    public async Task UpdateMember_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, UpdateMemberAsync);
        metrics.Stop();

        PrintSummary("PATCH MEMBERS - LOAD TEST", metrics);
    }

    /// <summary>Verifies that DELETE member under normal load stays within performance thresholds.</summary>
    [Fact]
    public async Task DeleteMember_UnderNormalLoad_PerformanceWithinThresholds()
    {
        var metrics = new MetricsCollector();
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(LoadTestConfig.Load.DurationSeconds));

        metrics.Start();
        await RunUsersAsync(LoadTestConfig.Load.Users, metrics, cts.Token,
            LoadTestConfig.Load.ThinkTimeMs, DeleteMemberAsync);
        metrics.Stop();

        PrintSummary("DELETE MEMBERS - LOAD TEST", metrics);
    }
}