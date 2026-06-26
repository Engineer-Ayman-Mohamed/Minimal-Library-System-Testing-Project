using LibrarySystem.Data.Entities;

namespace LibrarySystem.Services.services.interfaces;

public interface IBookService
{
    Task<Book?> GetByIdAsync(int id);
    Task<List<Book>> GetAllAsync();
    Task<List<Book>> GetAvailableBooksAsync();
    Task<Book?> GetByISBNAsync(string isbn);
    Task<Book> CreateAsync(string title, string author, string isbn, int totalCopies);
    Task<bool> ExistsByISBNAsync(string isbn);
    Task<Book> UpdateAsync(int id, string title, string author, string isbn, int totalCopies);
    Task DeleteAsync(int id);
    Task DecrementAvailableCopiesAsync(int bookId);
    Task IncrementAvailableCopiesAsync(int bookId);
}
