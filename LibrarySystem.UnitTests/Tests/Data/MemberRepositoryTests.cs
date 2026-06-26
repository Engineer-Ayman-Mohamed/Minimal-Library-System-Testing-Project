using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Repos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LibrarySystem.UnitTests.Tests.Data;

public class MemberRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LibrarySystemContext _context;
    private readonly MemberRepository _repository;
    public MemberRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibrarySystemContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LibrarySystemContext(options);
        _context.Database.EnsureCreated();
        _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        _repository = new MemberRepository(_context);
    }
    
    [Fact]
    public async Task AddAsync_DuplicateEmail_ThrowsDbUpdateException()
    {
        var member1 = new Member { FullName = "John", Email = "john@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1) };
        var member2 = new Member { FullName = "Jane", Email = "john@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1) };

        await _repository.AddAsync(member1);

        await Should.ThrowAsync<DbUpdateException>(
            async () => await _repository.AddAsync(member2)
        );
    }
    
    [Fact]
    public async Task AddAsync_DuplicateISBN_ThrowsDbUpdateException()
    {
        var bookRepo = new BookRepository(_context);
        var book1 = new Book { Title = "Book 1", Author = "Author", ISBN = "9780123456789", TotalCopies = 1, AvailableCopies = 1 };
        var book2 = new Book { Title = "Book 2", Author = "Author", ISBN = "9780123456789", TotalCopies = 1, AvailableCopies = 1 };

        await bookRepo.AddAsync(book1);

        await Should.ThrowAsync<DbUpdateException>(
            async () => await bookRepo.AddAsync(book2)
        );
    }
    
    [Fact]
    public async Task Delete_MemberWithActiveLoans_ThrowsException()
    {
        var loanRepo = new LoanRepository(_context);
        var member = new Member { Id = 10, FullName = "John", Email = "john@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1) };
        await _repository.AddAsync(member);

        var book = new Book { Id = 10, Title = "Book", Author = "Author", ISBN = "9780123456792", TotalCopies = 1, AvailableCopies = 0 };
        var bookRepo = new BookRepository(_context);
        await bookRepo.AddAsync(book);

        var loan = new Loan { BookId = 10, MemberId = 10, BorrowedAt = DateTime.Today, DueDate = DateTime.Today.AddDays(14) };
        await loanRepo.AddAsync(loan);

        await Should.ThrowAsync<Exception>(
            async () => await _repository.DeleteAsync(member.Id)
        );
    }
    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        _context.Dispose();
    }
}