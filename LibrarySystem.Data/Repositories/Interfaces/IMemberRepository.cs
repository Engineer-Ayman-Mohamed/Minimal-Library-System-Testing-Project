using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Repositories.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByEmailAsync(string email);
    Task<List<Member>> GetAllAsync();
    Task<Member> AddAsync(Member member);
    Task UpdateAsync(Member member);
    Task DeleteAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
}