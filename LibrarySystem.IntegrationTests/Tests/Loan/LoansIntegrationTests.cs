using System.Net;
using System.Net.Http.Json;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LibrarySystem.IntegrationTests.Tests.Loan;

/// <summary>Integration tests for the Loans API endpoints covering delete and update scenarios.</summary>
public class LoansIntegrationTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;
    public LoansIntegrationTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _fixture.LibrarySystemContext.Loans.RemoveRange(_fixture.LibrarySystemContext.Loans);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Books.RemoveRange(_fixture.LibrarySystemContext.Books);
        _fixture.LibrarySystemContext.Members.RemoveRange(_fixture.LibrarySystemContext.Members);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Books.AddRange(
            new Book { Id = 1, Title = "Test Book 1", Author = "Author 1", ISBN = "9780123456789", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Id = 2, Title = "Test Book 2", Author = "Author 2", ISBN = "9780123456790", TotalCopies = 1, AvailableCopies = 0 }
        );
        _fixture.LibrarySystemContext.Members.AddRange(
            new Member { Id = 1, FullName = "John Doe", Email = "john@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 }
        );
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Loans.AddRange(
            new Data.Entities.Loan { Id = 1, BookId = 1, MemberId = 1, BorrowedAt = DateTime.Today, DueDate = DateTime.Today.AddDays(14), ReturnedAt = null, FineAmount = 0 },
            new Data.Entities.Loan { Id = 2, BookId = 2, MemberId = 1, BorrowedAt = DateTime.Today.AddDays(-10), DueDate = DateTime.Today.AddDays(4), ReturnedAt = DateTime.Today, FineAmount = 0 }
        );
        await _fixture.LibrarySystemContext.SaveChangesAsync();
    }
    public Task DisposeAsync() => Task.CompletedTask;
    
    /// <summary>Verifies that PATCH on a returned loan returns NotFound.</summary>
    [Fact]
    public async Task Patch_ReturnedLoan_ReturnsNotFound()
    {
        var dto = new UpdateLoanDto { DueDate = DateTime.Today.AddDays(20) };

        var response = await _client.PatchAsJsonAsync("api/v1/Loans/2", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    /// <summary>Verifies that DELETE on an active loan returns NoContent.</summary>
    [Fact]
    public async Task Delete_ActiveLoan_ReturnsNoContent()
    {
        var response = await _client.DeleteAsync("api/v1/Loans/1");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>Verifies that DELETE on a non-existent loan returns NotFound.</summary>
    [Fact]
    public async Task Delete_NonExistentLoan_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("api/v1/Loans/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>Verifies that DELETE on a returned loan returns NotFound.</summary>
    [Fact]
    public async Task Delete_ReturnedLoan_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("api/v1/Loans/2");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>Verifies that deleting an active loan restores the book's available copies.</summary>
    [Fact]
    public async Task Delete_ActiveLoan_RestoresBookAvailability()
    {
        var response = await _client.DeleteAsync("api/v1/Loans/1");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var book = await _fixture.LibrarySystemContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == 1);

        book.ShouldNotBeNull();
        book.AvailableCopies.ShouldBe(3);
    }
}