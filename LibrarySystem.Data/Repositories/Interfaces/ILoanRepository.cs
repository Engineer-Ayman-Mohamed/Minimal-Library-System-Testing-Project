using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task<List<Loan>> GetAllAsync();
    Task<List<Loan>> GetActiveLoansForMemberAsync(int memberId);
    Task<int> GetActiveLoanCountForMemberAsync(int memberId);
    Task<Loan> AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
    Task<bool> HasActiveLoanForBookAsync(int bookId);
}