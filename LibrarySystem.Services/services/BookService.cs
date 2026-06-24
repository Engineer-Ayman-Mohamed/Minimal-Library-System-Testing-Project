using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Services.services.interfaces;

namespace LibrarySystem.Services.services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _bookRepository.GetByIdAsync(id);
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _bookRepository.GetAllAsync();
    }

    public async Task<List<Book>> GetAvailableBooksAsync()
    {
        return await _bookRepository.GetAvailableBooksAsync();
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        return await _bookRepository.GetByISBNAsync(isbn);
    }

    public async Task<Book> CreateAsync(string title, string author, string isbn, int totalCopies)
    {
        var book = new Book
        {
            Title = title,
            Author = author,
            ISBN = isbn,
            TotalCopies = totalCopies,
            AvailableCopies = totalCopies
        };

        return await _bookRepository.AddAsync(book);
    }

    public async Task<bool> ExistsByISBNAsync(string isbn)
    {
        return await _bookRepository.ExistsByISBNAsync(isbn);
    }

    public async Task DecrementAvailableCopiesAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book != null && book.AvailableCopies > 0)
        {
            book.AvailableCopies--;
            await _bookRepository.UpdateAsync(book);
        }
    }

    public async Task IncrementAvailableCopiesAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book != null)
        {
            book.AvailableCopies++;
            await _bookRepository.UpdateAsync(book);
        }
    }
}
