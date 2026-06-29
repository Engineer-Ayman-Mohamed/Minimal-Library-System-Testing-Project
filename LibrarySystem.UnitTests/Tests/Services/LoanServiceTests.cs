using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.services;
using Moq;
using Shouldly;

namespace LibrarySystem.UnitTests.Tests.Services;

/// <summary>Unit tests for LoanService borrow-book scenarios covering business rule validation.</summary>
public class LoanServiceTests
{
    private readonly Mock<ILoanRepository> _mockLoanRepo;
    private readonly Mock<IBookRepository> _mockBookRepo;
    private readonly Mock<IMemberRepository> _mockMemberRepo;
    private readonly LoanService _service;

    public LoanServiceTests()
    {
        _mockLoanRepo = new Mock<ILoanRepository>();
        _mockBookRepo = new Mock<IBookRepository>();
        _mockMemberRepo = new Mock<IMemberRepository>();
        _service = new LoanService(_mockLoanRepo.Object, _mockBookRepo.Object, _mockMemberRepo.Object);
    }
    
    /// <summary>Verifies that a member with 3 active loans cannot borrow another book.</summary>
    [Fact]
    public async Task BorrowBook_MemberHas3ActiveLoans_ThrowsLoanLimitExceededException()
    {
        var member = new Member { Id = 1, MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 };
        _mockMemberRepo.Setup(member => member.GetByIdAsync(1)).ReturnsAsync(member);
        _mockLoanRepo.Setup(loan => loan.GetActiveLoanCountForMemberAsync(1)).ReturnsAsync(3);

        await Should.ThrowAsync<LoanLimitExceededException>(
            async () => await _service.BorrowBookAsync(memberId: 1, bookId: 1)
        );
    }
    
    /// <summary>Verifies that borrowing a book with zero available copies throws BookNotAvailableException.</summary>
    [Fact]
    public async Task BorrowBook_NoAvailableCopies_ThrowsBookNotAvailableException()
    {
        var member = new Member { Id = 1, MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 };
        var book = new Book { Id = 1, AvailableCopies = 0 };

        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _mockLoanRepo.Setup(r => r.GetActiveLoanCountForMemberAsync(1)).ReturnsAsync(0);
        _mockBookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        await Should.ThrowAsync<BookNotAvailableException>(
            async () => await _service.BorrowBookAsync(memberId: 1, bookId: 1)
        );
    }
    
    /// <summary>Verifies that borrowing a book with an expired membership throws MembershipExpiredException.</summary>
    [Fact]
    public async Task BorrowBook_MembershipExpired_ThrowsMembershipExpiredException()
    {
        var member = new Member { Id = 1, MembershipExpiryDate = DateTime.Today.AddDays(-1), OutstandingFine = 0 };
        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        await Should.ThrowAsync<MembershipExpiredException>(
            async () => await _service.BorrowBookAsync(memberId: 1, bookId: 1)
        );
    }
    
    /// <summary>Verifies that a member with an outstanding fine cannot borrow a book.</summary>
    [Fact]
    public async Task BorrowBook_HasOutstandingFine_ThrowsOutstandingFineException()
    {
        var member = new Member { Id = 1, MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 5.00m };
        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        await Should.ThrowAsync<OutstandingFineException>(
            async () => await _service.BorrowBookAsync(memberId: 1, bookId: 1)
        );
    }
    
    /// <summary>Verifies that a successful borrow calls AddAsync on the loan repository exactly once.</summary>
    [Fact]
    public async Task BorrowBook_Successful_CallsAddAsyncOnce()
    {
        var member = new Member { Id = 1, MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 };
        var book = new Book { Id = 1, AvailableCopies = 3 };

        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _mockLoanRepo.Setup(r => r.GetActiveLoanCountForMemberAsync(1)).ReturnsAsync(0);
        _mockBookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        _mockLoanRepo.Setup(r => r.AddAsync(It.IsAny<Loan>())).ReturnsAsync(new Loan { Id = 1 });

        await _service.BorrowBookAsync(memberId: 1, bookId: 1);

        _mockLoanRepo.Verify(r => r.AddAsync(It.IsAny<Loan>()), Times.Once);
    }
    
    /// <summary>Verifies that a successful borrow decrements the book's available copies and updates the repository.</summary>
    [Fact]
    public async Task BorrowBook_Successful_DecrementsAvailableCopies()
    {
        var member = new Member { Id = 1, MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 };
        var book = new Book { Id = 1, AvailableCopies = 3 };

        _mockMemberRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);
        _mockLoanRepo.Setup(r => r.GetActiveLoanCountForMemberAsync(1)).ReturnsAsync(0);
        _mockBookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        _mockLoanRepo.Setup(r => r.AddAsync(It.IsAny<Loan>())).ReturnsAsync(new Loan { Id = 1 });

        await _service.BorrowBookAsync(memberId: 1, bookId: 1);

        book.AvailableCopies.ShouldBe(2);
        _mockBookRepo.Verify(r => r.UpdateAsync(book), Times.Once);
    }
}