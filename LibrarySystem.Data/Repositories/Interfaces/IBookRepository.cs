using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Repositories.Interfaces;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(int id);
    Task<Book?> GetByISBNAsync(string isbn);
    Task<List<Book>> GetAllAsync();
    Task<List<Book>> GetAvailableBooksAsync();
    Task<Book> AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(int id);
    Task<bool> ExistsByISBNAsync(string isbn);
}