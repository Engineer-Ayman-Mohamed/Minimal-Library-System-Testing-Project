using System.Net;
using System.Net.Http.Json;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.IntegrationTests.Fixtures;
using Shouldly;

namespace LibrarySystem.IntegrationTests.Tests.Books;

/// <summary>Integration tests for the Books API endpoints covering CRUD operations and validation.</summary>
public class BooksIntegrationTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;
    public BooksIntegrationTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _fixture.LibrarySystemContext.Books.AddRange(
            new Book { Id = 1, Title = "Test Book 1", Author = "Author 1", ISBN = "9780123456789", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Id = 2, Title = "Test Book 2", Author = "Author 2", ISBN = "9780123456790", TotalCopies = 1, AvailableCopies = 0 },
            new Book { Id = 3, Title = "Test Book 3", Author = "Author 3", ISBN = "9780123456791", TotalCopies = 2, AvailableCopies = 4 },
            new Book { Id = 4, Title = "Test Book 4", Author = "Author 4", ISBN = "9780123456792", TotalCopies = 3, AvailableCopies = 5 }
        );
        _fixture.LibrarySystemContext.Members.AddRange(
            new Member { Id = 1, FullName = "John Doe", Email = "john@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 }
        );
        
        await _fixture.LibrarySystemContext.SaveChangesAsync();
        _fixture.LibrarySystemContext.Loans.AddRange(
            new Data.Entities.Loan { Id = 1, BookId = 2, MemberId = 1, BorrowedAt = DateTime.Today, DueDate = DateTime.Today.AddDays(14), ReturnedAt = null, FineAmount = 0 }
        );
        await _fixture.LibrarySystemContext.SaveChangesAsync();
    }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Verifies that PATCH on an existing book returns OK with the updated fields.</summary>
    [Fact]
    public async Task Patch_ExistingBook_ReturnsOkWithUpdatedBook()
    {
        var dto = new UpdateBookDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            ISBN = "9780123456789",
            TotalCopies = 10
        };

        var request = await _client.PatchAsJsonAsync("api/v1/Books/1", dto);
        request.StatusCode.ShouldBe(HttpStatusCode.OK);

        var bookResponse = await request.Content.ReadFromJsonAsync<BookDto>();
        bookResponse.ShouldNotBeNull();
        bookResponse.Id.ShouldBe(1);
        bookResponse.Title.ShouldBe(dto.Title);
        bookResponse.Author.ShouldBe(dto.Author);
        bookResponse.ISBN.ShouldBe(dto.ISBN);
        bookResponse.TotalCopies.ShouldBe(dto.TotalCopies);
    }
    
    /// <summary>Verifies that GET all books returns OK with all seeded books.</summary>
    [Fact]
    public async Task GetAll_ReturnsOkWithAllBooks()
    {
        var response = await _client.GetAsync("api/v1/Books");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        books.ShouldNotBeNull();
        books.Count.ShouldBe(4);
    }
    
    /// <summary>Verifies that GET all with available filter returns only books with available copies.</summary>
    [Fact]
    public async Task GetAll_WithAvailableFilter_ReturnsOnlyAvailableBooks()
    {
        var response = await _client.GetAsync("api/v1/Books?available=true");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        books.ShouldNotBeNull();
        books.ShouldAllBe(b => b.AvailableCopies > 0);
        books.Count.ShouldBe(3);
        books[0].Id.ShouldBe(1);
    }
    
    /// <summary>Verifies that GET by ID for an existing book returns OK with the correct book data.</summary>
    [Fact]
    public async Task GetById_ExistingBook_ReturnsOkWithBook()
    {
        var response = await _client.GetAsync("api/v1/Books/1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        book.ShouldNotBeNull();
        book.Id.ShouldBe(1);
        book.Title.ShouldBe("Test Book 1");
        book.Author.ShouldBe("Author 1");
        book.ISBN.ShouldBe("9780123456789");
        book.TotalCopies.ShouldBe(3);
        book.AvailableCopies.ShouldBe(2);
    }
    
    /// <summary>Verifies that GET by ID for a non-existent book returns NotFound.</summary>
    [Fact]
    public async Task GetById_NonExistentBook_ReturnsNotFound()
    {
        var response = await _client.GetAsync("api/v1/Books/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    /// <summary>Verifies that POST with a valid book returns Created with the new book data.</summary>
    [Fact]
    public async Task Create_ValidBook_ReturnsCreatedWithBook()
    {
        var dto = new CreateBookDto
        {
            Title = "New Book",
            Author = "New Author",
            ISBN = "9780000000001",
            TotalCopies = 5
        };

        var response = await _client.PostAsJsonAsync("api/v1/Books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        book.ShouldNotBeNull();
        book.Title.ShouldBe(dto.Title);
        book.Author.ShouldBe(dto.Author);
        book.ISBN.ShouldBe(dto.ISBN);
        book.TotalCopies.ShouldBe(dto.TotalCopies);
        book.AvailableCopies.ShouldBe(dto.TotalCopies);
    }
    
    /// <summary>Verifies that POST with a duplicate ISBN returns Conflict.</summary>
    [Fact]
    public async Task Create_DuplicateISBN_ReturnsConflict()
    {
        var dto = new CreateBookDto
        {
            Title = "Duplicate Book",
            Author = "Some Author",
            ISBN = "9780123456789",
            TotalCopies = 2
        };

        var response = await _client.PostAsJsonAsync("api/v1/Books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
    
    /// <summary>Verifies that POST with an invalid ISBN returns BadRequest.</summary>
    [Fact]
    public async Task Create_InvalidISBN_ReturnsBadRequest()
    {
        var dto = new CreateBookDto
        {
            Title = "Bad Book",
            Author = "Some Author",
            ISBN = "123",
            TotalCopies = 1
        };

        var response = await _client.PostAsJsonAsync("api/v1/Books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    /// <summary>Verifies that POST with zero total copies returns BadRequest.</summary>
    [Fact]
    public async Task Create_ZeroTotalCopies_ReturnsBadRequest()
    {
        var dto = new CreateBookDto
        {
            Title = "Bad Book",
            Author = "Some Author",
            ISBN = "9780000000002",
            TotalCopies = 0
        };

        var response = await _client.PostAsJsonAsync("api/v1/Books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    /// <summary>Verifies that PATCH on a non-existent book returns NotFound.</summary>
    [Fact]
    public async Task Patch_NonExistentBook_ReturnsNotFound()
    {
        var dto = new UpdateBookDto
        {
            Title = "Updated Title",
            Author = "Updated Author",
            ISBN = "9780000000003",
            TotalCopies = 5
        };

        var response = await _client.PatchAsJsonAsync("api/v1/Books/999", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    /// <summary>Verifies that DELETE on an existing book returns NoContent.</summary>
    [Fact]
    public async Task Delete_ExistingBook_ReturnsNoContent()
    {
        var response = await _client.DeleteAsync("api/v1/Books/3");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
     
    /// <summary>Verifies that DELETE on a non-existent book returns NotFound.</summary>
    [Fact]
    public async Task Delete_NonExistentBook_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("api/v1/Books/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    /// <summary>Verifies that a deleted book is no longer retrievable via GET.</summary>
    [Fact]
    public async Task Delete_ExistingBook_IsNoLongerRetrievable()
    {
        var deleteResponse = await _client.DeleteAsync("api/v1/Books/4");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync("api/v1/Books/4");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    /// <summary>Verifies that a newly created book is immediately retrievable via GET.</summary>
    [Fact]
    public async Task Create_ValidBook_IsRetrievableAfterCreation()
    {
        var dto = new CreateBookDto
        {
            Title = "Retrievable Book",
            Author = "Some Author",
            ISBN = "9780000000004",
            TotalCopies = 3
        };

        var createResponse = await _client.PostAsJsonAsync("api/v1/Books", dto);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<BookDto>();
        created.ShouldNotBeNull();

        var getResponse = await _client.GetAsync($"api/v1/Books/{created.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var retrieved = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(created.Id);
        retrieved.Title.ShouldBe(dto.Title);
        retrieved.ISBN.ShouldBe(dto.ISBN);
    }
}