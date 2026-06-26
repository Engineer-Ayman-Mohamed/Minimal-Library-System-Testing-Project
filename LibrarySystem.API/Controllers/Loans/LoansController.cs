using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers.Loans;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;
    public LoansController(ILoanService loanService, IMapper mapper)
    {
        _loanService = loanService;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<LoanDto>>> GetByMember([FromQuery] int memberId)
    {
        var loans = await _loanService.GetAllLoansForMemberAsync(memberId);
        return Ok(_mapper.Map<List<LoanDto>>(loans));
    }
    
    [HttpPost]
    public async Task<ActionResult<LoanDto>> Borrow([FromBody] CreateLoanDto dto)
    {
        try
        {
            var loan = await _loanService.BorrowBookAsync(dto.MemberId, dto.BookId);
            return CreatedAtAction(nameof(GetByMember), new { memberId = dto.MemberId }, _mapper.Map<LoanDto>(loan));
        }
        catch (MembershipExpiredException ex) { return UnprocessableEntity(new { message = ex.Message }); }
        catch (OutstandingFineException ex) { return UnprocessableEntity(new { message = ex.Message }); }
        catch (LoanLimitExceededException ex) { return UnprocessableEntity(new { message = ex.Message }); }
        catch (BookNotAvailableException ex) { return UnprocessableEntity(new { message = ex.Message }); }
    }

    [HttpPut("{id}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
    {
        try
        {
            var loan = await _loanService.ReturnBookAsync(id);
            return Ok(_mapper.Map<LoanDto>(loan));
        }
        catch (AlreadyReturnedException ex) { return UnprocessableEntity(new { message = ex.Message }); }
    }
}