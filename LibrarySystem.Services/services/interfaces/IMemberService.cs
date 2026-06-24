using LibrarySystem.Data.Entities;

namespace LibrarySystem.Services.services.interfaces;

public interface IMemberService
{
    Task<Member?> GetByIdAsync(int id);
    Task<List<Member>> GetAllAsync();
    Task<Member?> GetByEmailAsync(string email);
    Task<Member> CreateAsync(string fullName, string email, DateTime membershipExpiryDate);
    Task<bool> ExistsByEmailAsync(string email);
    Task UpdateAsync(Member member);
}
