using LibrarySystem.Data.Entities;

namespace LibrarySystem.Services.services.interfaces;

public interface ILoanService
{
    Task<Loan> BorrowBookAsync(int memberId, int bookId);
    Task<Loan> ReturnBookAsync(int loanId);
    Task<Loan?> GetByIdAsync(int loanId);
    Task<List<Loan>> GetActiveLoansForMemberAsync(int memberId);
    Task<List<Loan>> GetAllLoansForMemberAsync(int memberId);
    decimal CalculateFine(DateTime borrowedAt, DateTime returnedAt);
}
