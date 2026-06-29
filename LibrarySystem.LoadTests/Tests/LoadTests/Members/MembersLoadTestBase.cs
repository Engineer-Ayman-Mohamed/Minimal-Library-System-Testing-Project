using System.Diagnostics;
using System.Net.Http.Json;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Members;

public class MembersLoadTestBase : IAsyncLifetime
{
    protected readonly LoadTestBase _fixture;
    protected readonly ITestOutputHelper _output;
    protected HttpClient Client => _fixture.Client;
    
    private int _createCounter;
    private readonly object _createLock = new();
    protected MembersLoadTestBase(LoadTestBase fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _fixture.LibrarySystemContext.Loans.RemoveRange(_fixture.LibrarySystemContext.Loans);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Books.RemoveRange(_fixture.LibrarySystemContext.Books);
        _fixture.LibrarySystemContext.Members.RemoveRange(_fixture.LibrarySystemContext.Members);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        var members = Enumerable.Range(1, LoadTestConfig.Seed.Members).Select(i => new Member
        {
            Id = i,
            FullName = $"Member {i}",
            Email = $"member{i}@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1),
            OutstandingFine = 0
        });
        _fixture.LibrarySystemContext.Members.AddRange(members);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _createCounter = LoadTestConfig.Seed.Members;
    }
    
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task RunUsersAsync(
        int userCount, MetricsCollector metrics,
        CancellationToken ct, int thinkTimeMs,
        Func<int, Task<(int StatusCode, long Ms)>> requestFn
    ) {
        var tasks = Enumerable.Range(1, userCount)
            .Select(userId => RunSingleUserAsync(userId, metrics, ct, thinkTimeMs, requestFn));
        await Task.WhenAll(tasks);
    }

    private async Task RunSingleUserAsync(
        int userId, MetricsCollector metrics,
        CancellationToken ct, int thinkTimeMs,
        Func<int, Task<(int StatusCode, long Ms)>> requestFn
    ) {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var (statusCode, ms) = await requestFn(userId);
                metrics.Record(statusCode, ms);
            }
            catch (OperationCanceledException) { break; }
            catch { metrics.Record(500, 0); }

            await Task.Delay(thinkTimeMs, CancellationToken.None);
        }
    }

    protected async Task<(int StatusCode, long Ms)> GetMemberByIdAsync(int userId)
    {
        var memberId = (userId % LoadTestConfig.Seed.Members) + 1;
        var sw = Stopwatch.StartNew();
        using var response = await Client.GetAsync($"/api/v1/Members/{memberId}");
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    protected async Task<(int StatusCode, long Ms)> CreateMemberAsync(int userId)
    {
        int counter;
        lock (_createLock) { counter = ++_createCounter; }

        var dto = new CreateMemberDto
        {
            FullName = $"Load Member {counter}",
            Email = $"loadmember{counter}@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var sw = Stopwatch.StartNew();
        using var response = await Client.PostAsJsonAsync("/api/v1/Members", dto);
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    protected async Task<(int StatusCode, long Ms)> UpdateMemberAsync(int userId)
    {
        var memberId = (userId % LoadTestConfig.Seed.Members) + 1;

        var dto = new UpdateMemberDto
        {
            FullName = $"Updated Member {memberId}",
            Email = $"updated{memberId}@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(2)
        };

        var sw = Stopwatch.StartNew();
        using var response = await Client.PatchAsJsonAsync($"/api/v1/Members/{memberId}", dto);
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    protected async Task<(int StatusCode, long Ms)> DeleteMemberAsync(int userId)
    {
        var memberId = (userId % LoadTestConfig.Seed.Members) + 1;

        var sw = Stopwatch.StartNew();
        using var response = await Client.DeleteAsync($"/api/v1/Members/{memberId}");
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }
    protected void PrintSummary(string label, MetricsCollector metrics)
        => _output.WriteLine($"\n=== {label} ===\n{metrics.Summary()}");
}