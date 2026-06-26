using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Repos;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LibrarySystem.UnitTests.Tests.Data;

public class BookRepositoryTests : IDisposable
{
    private readonly LibrarySystemContext _context;
    private readonly BookRepository _repository;

    public BookRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LibrarySystemContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new LibrarySystemContext(options);
        _repository = new BookRepository(_context);
        SeedData();
    }
    private void SeedData()
    {
        _context.Books.AddRange(
            new Book { Id = 1, Title = "Available Book 1", Author = "Author 1", ISBN = "9780123456789", TotalCopies = 5, AvailableCopies = 3 },
            new Book { Id = 2, Title = "Available Book 2", Author = "Author 2", ISBN = "9780123456790", TotalCopies = 3, AvailableCopies = 1 },
            new Book { Id = 3, Title = "Unavailable Book", Author = "Author 3", ISBN = "9780123456791", TotalCopies = 2, AvailableCopies = 0 }
        );
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAvailableBooksAsync_ReturnsOnlyAvailable()
    {
        var result = await _repository.GetAvailableBooksAsync();

        result.Count.ShouldBe(2);
        result.ShouldAllBe(b => b.AvailableCopies > 0);
    }
    
    [Fact]
    public async Task GetByISBNAsync_WhenExists_ReturnsBook()
    {
        var result = await _repository.GetByISBNAsync("9780123456789");

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Available Book 1");
    }
    
    [Fact]
    public async Task GetByISBNAsync_WhenNotExists_ReturnsNull()
    {
        var result = await _repository.GetByISBNAsync("0000000000000");

        result.ShouldBeNull();
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        var result = await _repository.GetAllAsync();

        result.Count.ShouldBe(3);
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}