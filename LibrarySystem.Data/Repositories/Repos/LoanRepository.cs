using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.Repositories.Repos;

public class LoanRepository : ILoanRepository
{
    private readonly LibrarySystemContext _context;

    public LoanRepository(LibrarySystemContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Loan>> GetAllAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetActiveLoansForMemberAsync(int memberId)
    {
        return await _context.Loans
            .Where(l => l.MemberId == memberId && l.ReturnedAt == null)
            .Include(l => l.Book)
            .ToListAsync();
    }

    public async Task<int> GetActiveLoanCountForMemberAsync(int memberId)
    {
        return await _context.Loans
            .CountAsync(l => l.MemberId == memberId && l.ReturnedAt == null);
    }

    public async Task<Loan> AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
        return loan;
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasActiveLoanForBookAsync(int bookId)
    {
        return await _context.Loans.AnyAsync(l => l.BookId == bookId && l.ReturnedAt == null);
    }

    public async Task DeleteAsync(int id)
    {
        var loan = await _context.Loans.FindAsync(id);
        if (loan != null)
        {
            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
        }
    }
}