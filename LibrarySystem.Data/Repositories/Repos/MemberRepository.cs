using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.Repositories.Repos;

public class MemberRepository : IMemberRepository
{
    private readonly LibrarySystemContext _context;

    public MemberRepository(LibrarySystemContext context)
    {
        _context = context;
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members.FindAsync(id);
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.Email == email);
    }

    public async Task<List<Member>> GetAllAsync()
    {
        return await _context.Members.ToListAsync();
    }

    public async Task<Member> AddAsync(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task UpdateAsync(Member member)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member != null)
        {
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Members.AnyAsync(m => m.Email == email);
    }
}