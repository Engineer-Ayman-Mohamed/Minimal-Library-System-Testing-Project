using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Seeders;

public class DataSeeder
{
    private readonly LibrarySystemContext _context;

    public DataSeeder(LibrarySystemContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        if (await _context.Books.AnyAsync())
            return;
        
        await SeedBooksAsync();
        await _context.SaveChangesAsync();

        await SeedMembersAsync();
        await _context.SaveChangesAsync();

        await SeedLoansAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedBooksAsync()
    {
        var books = new List<Book>
        {
            new Book { Title = "Harry Potter and the Philosopher's Stone", Author = "J.K. Rowling", ISBN = "9780747532743", TotalCopies = 5, AvailableCopies = 4 },
            new Book { Title = "The Lord of the Rings", Author = "J.R.R. Tolkien", ISBN = "9780618640157", TotalCopies = 3, AvailableCopies = 3 },
            new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", ISBN = "9780547928227", TotalCopies = 4, AvailableCopies = 3 },
            new Book { Title = "1984", Author = "George Orwell", ISBN = "9780451524935", TotalCopies = 6, AvailableCopies = 5 },
            new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", ISBN = "9780061120084", TotalCopies = 4, AvailableCopies = 4 },
            new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", ISBN = "9780743273565", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Title = "Pride and Prejudice", Author = "Jane Austen", ISBN = "9780141439518", TotalCopies = 5, AvailableCopies = 5 },
            new Book { Title = "The Catcher in the Rye", Author = "J.D. Salinger", ISBN = "9780316769488", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Title = "Animal Farm", Author = "George Orwell", ISBN = "9780451526342", TotalCopies = 4, AvailableCopies = 3 },
            new Book { Title = "Brave New World", Author = "Aldous Huxley", ISBN = "9780060850524", TotalCopies = 3, AvailableCopies = 3 }
        };

        await _context.Books.AddRangeAsync(books);
    }

    private async Task SeedMembersAsync()
    {
        var members = new List<Member>
        {
            new Member { FullName = "John Smith", Email = "john.smith@email.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 },
            new Member { FullName = "Jane Doe", Email = "jane.doe@email.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 },
            new Member { FullName = "Bob Johnson", Email = "bob.johnson@email.com", MembershipExpiryDate = DateTime.Today.AddMonths(6), OutstandingFine = 2.50m },
            new Member { FullName = "Alice Williams", Email = "alice.williams@email.com", MembershipExpiryDate = DateTime.Today.AddYears(2), OutstandingFine = 0 },
            new Member { FullName = "Charlie Brown", Email = "charlie.brown@email.com", MembershipExpiryDate = DateTime.Today.AddDays(-30), OutstandingFine = 0 }
        };

        await _context.Members.AddRangeAsync(members);
    }

    private async Task SeedLoansAsync()
    {
        var john = await _context.Members.FirstOrDefaultAsync(m => m.Email == "john.smith@email.com");
        var jane = await _context.Members.FirstOrDefaultAsync(m => m.Email == "jane.doe@email.com");

        var book1 = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == "9780747532743");
        var book2 = await _context.Books.FirstOrDefaultAsync(b => b.ISBN == "9780618640157");

        if (john == null || jane == null || book1 == null || book2 == null)
            return;

        var loans = new List<Loan>
        {
            new Loan
            {
                Book = book1,
                Member = john,
                BorrowedAt = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(4),
                ReturnedAt = null,
                FineAmount = 0
            },
            new Loan
            {
                Book = book2,
                Member = jane,
                BorrowedAt = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-6),
                ReturnedAt = DateTime.Today.AddDays(-2),
                FineAmount = 2.00m
            }
        };
        await _context.Loans.AddRangeAsync(loans);
    }
}
