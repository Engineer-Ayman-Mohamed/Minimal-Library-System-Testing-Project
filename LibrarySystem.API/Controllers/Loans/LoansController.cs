using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers.Loans;

/// <summary>
///     Handles loan operations: borrow, return, update, delete, and query by member.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoansController" /> class.
    /// </summary>
    public LoansController(ILoanService loanService, IMapper mapper)
    {
        _loanService = loanService;
        _mapper = mapper;
    }
    
    /// <summary>Returns all loans for a given member.</summary>
    /// <param name="memberId">The member identifier.</param>
    [HttpGet]
    public async Task<ActionResult<List<LoanDto>>> GetByMember([FromQuery] int memberId)
    {
        var loans = await _loanService.GetAllLoansForMemberAsync(memberId);
        return Ok(_mapper.Map<List<LoanDto>>(loans));
    }
    
    /// <summary>Borrows a book for a member.</summary>
    /// <param name="dto">The loan creation payload.</param>
    /// <returns>201 on success, 422 if any business rule is violated.</returns>
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

    /// <summary>Returns a borrowed book.</summary>
    /// <param name="id">The loan identifier.</param>
    /// <returns>200 with fine info, or 422 if already returned.</returns>
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

    /// <summary>Updates a loan's due date.</summary>
    /// <param name="id">The loan identifier.</param>
    /// <param name="dto">Payload with new due date.</param>
    /// <returns>200 on success, or 404 if not found or already returned.</returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<LoanDto>> Update(int id, [FromBody] UpdateLoanDto dto)
    {
        try
        {
            var loan = await _loanService.UpdateAsync(id, dto.DueDate);
            return Ok(_mapper.Map<LoanDto>(loan));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Deletes a loan and restores book availability.</summary>
    /// <param name="id">The loan identifier.</param>
    /// <returns>204 on success, or 404 if not found or already returned.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _loanService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}