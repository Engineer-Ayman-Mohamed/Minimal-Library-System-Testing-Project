using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.services;
using Moq;
using Shouldly;

namespace LibrarySystem.UnitTests.Tests.Services;

public class LoanServiceFineTests
{
    private readonly Mock<ILoanRepository> _mockLoanRepo;
    private readonly Mock<IBookRepository> _mockBookRepo;
    private readonly Mock<IMemberRepository> _mockMemberRepo;
    private readonly LoanService _service;

    public LoanServiceFineTests()
    {
        _mockLoanRepo = new Mock<ILoanRepository>();
        _mockBookRepo = new Mock<IBookRepository>();
        _mockMemberRepo = new Mock<IMemberRepository>();
        _service = new LoanService(_mockLoanRepo.Object, _mockBookRepo.Object, _mockMemberRepo.Object);
    }
    
    [Theory]
    [InlineData(0, 0.00)]   // 0 days late = £0.00
    [InlineData(1, 0.50)]   // 1 day late = £0.50
    [InlineData(3, 1.50)]   // 3 days late = £1.50
    [InlineData(14, 7.00)]  // 14 days late = £7.00
    public void CalculateFine_ReturnsCorrectAmount(int daysLate, decimal expectedFine)
    {
        var borrowedAt = new DateTime(2026, 1, 1);
        var returnedAt = borrowedAt.AddDays(14 + daysLate);

        var result = _service.CalculateFine(borrowedAt, returnedAt);

        result.ShouldBe(expectedFine);
    }

    [Fact]
    public void CalculateFine_ReturnedBeforeDueDate_ReturnsZero()
    {
        var borrowedAt = DateTime.Today.AddDays(-10);
        var returnedAt = DateTime.Today; // Only 10 days, loan is 14 days

        var result = _service.CalculateFine(borrowedAt, returnedAt);

        result.ShouldBe(0m);
    }
    
    [Fact]
    public async Task ReturnBook_LateReturn_UpdatesMemberOutstandingFine()
    {
        var member = new Member { Id = 1, OutstandingFine = 0 };
        var book = new Book { Id = 1, AvailableCopies = 2 };
        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            BorrowedAt = DateTime.Today.AddDays(-28),
            DueDate = DateTime.Today.AddDays(-6),
            ReturnedAt = null,
            FineAmount = 0
        };

        _mockLoanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loan);
        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _mockBookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        
        await _service.ReturnBookAsync(loanId: 1);

        member.OutstandingFine.ShouldBe(7.00m);
        
        _mockMemberRepo.Verify(r => r.UpdateAsync(member), Times.Once);
    }
    
    [Fact]
    public async Task ReturnBook_AlreadyReturned_ThrowsAlreadyReturnedException()
    {
        var loan = new Loan
        {
            Id = 1,
            ReturnedAt = DateTime.Today
        };

        _mockLoanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loan);

        await Should.ThrowAsync<AlreadyReturnedException>(
            async () => await _service.ReturnBookAsync(loanId: 1)
        );
    }
    
    [Fact]
    public async Task ReturnBook_OnTime_FineAmountIsZero()
    {
        var member = new Member { Id = 1, OutstandingFine = 0 };
        var book = new Book { Id = 1, AvailableCopies = 2 };
        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            BorrowedAt = DateTime.Today.AddDays(-10),
            DueDate = DateTime.Today.AddDays(4),
            ReturnedAt = null,
            FineAmount = 0
        };

        _mockLoanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loan);
        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _mockBookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        var result = await _service.ReturnBookAsync(loanId: 1);

        result.FineAmount.ShouldBe(0m);
    }
}