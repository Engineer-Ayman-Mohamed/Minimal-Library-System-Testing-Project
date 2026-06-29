using System.Diagnostics;
using System.Net.Http.Json;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Loans;

/// <summary>
///     Base class for Loans load/stress/spike tests.
///     Seeds 60 books, 30 members, and 30 active loans, then provides
///     concurrent HTTP methods for all loan endpoints.
/// </summary>
public class LoansLoadTestBase : IAsyncLifetime
{
    protected readonly LoadTestBase _fixture;
    protected readonly ITestOutputHelper _output;
    protected HttpClient Client => _fixture.Client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoansLoadTestBase" /> class.
    /// </summary>
    protected LoansLoadTestBase(LoadTestBase fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    /// <summary>Clears existing data and seeds books, members, and one active loan per member.</summary>
    public async Task InitializeAsync()
    {
        _fixture.LibrarySystemContext.Loans.RemoveRange(_fixture.LibrarySystemContext.Loans);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Books.RemoveRange(_fixture.LibrarySystemContext.Books);
        _fixture.LibrarySystemContext.Members.RemoveRange(_fixture.LibrarySystemContext.Members);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        var books = Enumerable.Range(1, LoadTestConfig.Seed.Books).Select(i => new Book
        {
            Id = i,
            Title = $"Book {i}",
            Author = $"Author {i}",
            ISBN = $"{9780000000000 + i}",
            TotalCopies = 10,
            AvailableCopies = 10
        });
        _fixture.LibrarySystemContext.Books.AddRange(books);

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

        var loans = Enumerable.Range(1, LoadTestConfig.Seed.Members).Select(i => new Loan
        {
            Id = i,
            BookId = i,
            MemberId = i,
            BorrowedAt = DateTime.Today,
            DueDate = DateTime.Today.AddDays(14),
            ReturnedAt = null,
            FineAmount = 0
        });
        _fixture.LibrarySystemContext.Loans.AddRange(loans);
        await _fixture.LibrarySystemContext.SaveChangesAsync();
    }
    
    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
    
    /// <summary>Runs a number of concurrent simulated users, each calling the given request function.</summary>
    protected async Task RunUsersAsync(
        int userCount, MetricsCollector metrics,
        CancellationToken ct, int thinkTimeMs, Func<int, 
        Task<(int StatusCode, long Ms)>> requestFn
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

    /// <summary>Gets all loans for a member, cycling through seeded members.</summary>
    protected async Task<(int StatusCode, long Ms)> GetLoansForMemberAsync(int userId)
    {
        var memberId = (userId % LoadTestConfig.Seed.Members) + 1;
        var sw = Stopwatch.StartNew();
        using var response = await Client.GetAsync($"/api/v1/Loans?memberId={memberId}");
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    /// <summary>Borrows a book, cycling through unique book/member pairs.</summary>
    protected async Task<(int StatusCode, long Ms)> BorrowBookAsync(int userId)
    {
        var bookId = (userId % (LoadTestConfig.Seed.Books - LoadTestConfig.Seed.Members))
                     + LoadTestConfig.Seed.Members + 1;
        var memberId = (userId % LoadTestConfig.Seed.Members) + 1;

        var dto = new CreateLoanDto { MemberId = memberId, BookId = bookId };
        var sw = Stopwatch.StartNew();
        using var response = await Client.PostAsJsonAsync("/api/v1/Loans", dto);
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    /// <summary>Updates a loan's due date, cycling through seeded loans.</summary>
    protected async Task<(int StatusCode, long Ms)> PatchLoanAsync(int userId)
    {
        var loanId = (userId % LoadTestConfig.Seed.Members) + 1;
        var dto = new UpdateLoanDto { DueDate = DateTime.Today.AddDays(21) };
        var sw = Stopwatch.StartNew();
        using var response = await Client.PatchAsJsonAsync($"/api/v1/Loans/{loanId}", dto);
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    /// <summary>Returns a borrowed book, cycling through seeded loans.</summary>
    protected async Task<(int StatusCode, long Ms)> ReturnBookAsync(int userId)
    {
        var loanId = (userId % LoadTestConfig.Seed.Members) + 1;
        var sw = Stopwatch.StartNew();
        using var response = await Client.PutAsJsonAsync($"/api/v1/Loans/{loanId}/return", new { });
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    /// <summary>Deletes a loan, cycling through seeded loans.</summary>
    protected async Task<(int StatusCode, long Ms)> DeleteLoanAsync(int userId)
    {
        var loanId = (userId % LoadTestConfig.Seed.Members) + 1;
        var sw = Stopwatch.StartNew();
        using var response = await Client.DeleteAsync($"/api/v1/Loans/{loanId}");
        sw.Stop();
        return ((int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    /// <summary>Asserts that average response time and P95 are within configured thresholds.</summary>
    protected void AssertLoadThresholds(MetricsCollector metrics, string label)
    {
        var failures = new List<string>();

        if (metrics.AverageMs > LoadTestConfig.Thresholds.MaxAverageMs)
            failures.Add($"[{label}] Avg {metrics.AverageMs:F0}ms > {LoadTestConfig.Thresholds.MaxAverageMs}ms");

        if (metrics.Percentile(95) > LoadTestConfig.Thresholds.MaxP95Ms)
            failures.Add($"[{label}] P95 {metrics.Percentile(95)}ms > {LoadTestConfig.Thresholds.MaxP95Ms}ms");

        Assert.False(failures.Any(), $"Load thresholds violated:\n" + string.Join('\n', failures));
    }

    /// <summary>Prints a labelled metrics summary to the test output.</summary>
    protected void PrintSummary(string label, MetricsCollector metrics)
        => _output.WriteLine($"\n=== {label} ===\n{metrics.Summary()}");
}
