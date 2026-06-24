using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.services.interfaces;

namespace LibrarySystem.Services.services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;

    private const int MAX_ACTIVE_LOANS = 3;
    private const decimal FINE_PER_DAY = 0.50m;
    private const int LOAN_DURATION_DAYS = 14;

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
    }

    public async Task<Loan> BorrowBookAsync(int memberId, int bookId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member == null)
            throw new InvalidOperationException("Member not found.");

        if (member.MembershipExpiryDate < DateTime.Today)
            throw new MembershipExpiredException();

        if (member.OutstandingFine > 0)
            throw new OutstandingFineException();

        var activeLoanCount = await _loanRepository.GetActiveLoanCountForMemberAsync(memberId);
        if (activeLoanCount >= MAX_ACTIVE_LOANS)
            throw new LoanLimitExceededException();

        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null)
            throw new InvalidOperationException("Book not found.");

        if (book.AvailableCopies <= 0)
            throw new BookNotAvailableException();

        var loan = new Loan
        {
            BookId = bookId,
            MemberId = memberId,
            BorrowedAt = DateTime.Today,
            DueDate = DateTime.Today.AddDays(LOAN_DURATION_DAYS),
            ReturnedAt = null,
            FineAmount = 0
        };

        await _loanRepository.AddAsync(loan);

        book.AvailableCopies--;
        await _bookRepository.UpdateAsync(book);

        return loan;
    }

    public async Task<Loan> ReturnBookAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan == null)
            throw new InvalidOperationException("Loan not found.");

        if (loan.ReturnedAt != null)
            throw new AlreadyReturnedException();

        loan.ReturnedAt = DateTime.Today;

        if (loan.ReturnedAt > loan.DueDate)
        {
            loan.FineAmount = CalculateFine(loan.BorrowedAt, loan.ReturnedAt.Value);

            var member = await _memberRepository.GetByIdAsync(loan.MemberId);
            if (member != null)
            {
                member.OutstandingFine += loan.FineAmount;
                await _memberRepository.UpdateAsync(member);
            }
        }

        await _loanRepository.UpdateAsync(loan);

        var book = await _bookRepository.GetByIdAsync(loan.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
            await _bookRepository.UpdateAsync(book);
        }

        return loan;
    }

    public async Task<Loan?> GetByIdAsync(int loanId)
    {
        return await _loanRepository.GetByIdAsync(loanId);
    }

    public async Task<List<Loan>> GetActiveLoansForMemberAsync(int memberId)
    {
        return await _loanRepository.GetActiveLoansForMemberAsync(memberId);
    }

    public async Task<List<Loan>> GetAllLoansForMemberAsync(int memberId)
    {
        var allLoans = await _loanRepository.GetAllAsync();
        return allLoans.Where(l => l.MemberId == memberId)
                       .OrderByDescending(l => l.BorrowedAt)
                       .ToList();
    }

    public decimal CalculateFine(DateTime borrowedAt, DateTime returnedAt)
    {
        if (returnedAt <= borrowedAt.AddDays(LOAN_DURATION_DAYS))
            return 0;

        var daysLate = (returnedAt - borrowedAt.AddDays(LOAN_DURATION_DAYS)).Days;
        return daysLate * FINE_PER_DAY;
    }
}
