using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.Services.services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers.Members;

/// <summary>
///     Handles CRUD operations for the Members resource.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MembersController" /> class.
    /// </summary>
    public MembersController(IMemberService memberService, ILoanService loanService, IMapper mapper)
    {
        _memberService = memberService;
        _loanService = loanService;
        _mapper = mapper;
    }
    
    /// <summary>Returns a single member by their ID, including active loan count.</summary>
    /// <param name="id">The member identifier.</param>
    /// <returns>200 with the member, or 404 if not found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var dto = _mapper.Map<MemberDto>(member);
        dto.ActiveLoanCount = (await _loanService.GetActiveLoansForMemberAsync(id)).Count;

        return Ok(dto);
    }
    
    /// <summary>Creates a new member.</summary>
    /// <param name="dto">The member creation payload.</param>
    /// <returns>201 with the created member, or 409 if the email already exists.</returns>
    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create([FromBody] CreateMemberDto dto)
    {
        if (await _memberService.ExistsByEmailAsync(dto.Email))
            return Conflict(new { message = "A member with this email already exists." });

        var member = _mapper.Map<Member>(dto);
        var created = await _memberService.CreateAsync(member.FullName, member.Email, member.MembershipExpiryDate);

        var result = _mapper.Map<MemberDto>(created);
        result.ActiveLoanCount = 0;

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates an existing member.</summary>
    /// <param name="id">The member identifier.</param>
    /// <param name="dto">The updated member data.</param>
    /// <returns>200 with the updated member, or 404 if not found.</returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<MemberDto>> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        member.FullName = dto.FullName;
        member.Email = dto.Email;
        member.MembershipExpiryDate = dto.MembershipExpiryDate;

        await _memberService.UpdateAsync(member);

        var result = _mapper.Map<MemberDto>(member);
        result.ActiveLoanCount = (await _loanService.GetActiveLoansForMemberAsync(id)).Count;
        return Ok(result);
    }

    /// <summary>Deletes a member by their ID.</summary>
    /// <param name="id">The member identifier.</param>
    /// <returns>204 on success, or 404 if not found.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        await _memberService.DeleteAsync(id);
        return NoContent();
    }
}