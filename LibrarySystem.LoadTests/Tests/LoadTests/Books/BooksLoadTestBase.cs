using System.Diagnostics;
using LibrarySystem.Data.Entities;
using LibrarySystem.LoadTests.Configs;
using LibrarySystem.LoadTests.Fixtures;
using LibrarySystem.LoadTests.Helpers;
using Xunit.Abstractions;

namespace LibrarySystem.LoadTests.Tests.LoadTests.Books;

/// <summary>
///     Base class for Books load/stress/spike tests.
///     Seeds 60 books and 30 members, then provides concurrent GET /api/v1/Books execution.
/// </summary>
public abstract class BooksLoadTestBase : IAsyncLifetime
{
    protected readonly LoadTestBase _fixture;
    protected readonly ITestOutputHelper _output;
    protected HttpClient Client => _fixture.Client;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BooksLoadTestBase" /> class.
    /// </summary>
    protected BooksLoadTestBase(LoadTestBase fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    /// <summary>Clears existing data and seeds fresh books and members.</summary>
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
            TotalCopies = 5,
            AvailableCopies = 5
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
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Runs a number of concurrent simulated users with the given think time.</summary>
    protected async Task RunUsersAsync(
        int userCount, MetricsCollector metrics,
        CancellationToken ct, int thinkTimeMs
    ) {
        var tasks = Enumerable.Range(1, userCount)
            .Select(_ => RunSingleUserAsync(metrics, ct, thinkTimeMs));
        await Task.WhenAll(tasks);
    }

    private async Task RunSingleUserAsync(
        MetricsCollector metrics,
        CancellationToken ct, int thinkTimeMs
    ) {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                using var response = await Client.GetAsync("/api/v1/Books", ct);
                sw.Stop();
                metrics.Record((int)response.StatusCode, sw.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) { break; }
            catch { metrics.Record(500, 0); }

            await Task.Delay(thinkTimeMs, CancellationToken.None);
        }
    }
}