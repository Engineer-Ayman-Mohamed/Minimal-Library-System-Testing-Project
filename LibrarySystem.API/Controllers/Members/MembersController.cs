using AutoMapper;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.Services.services.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers.Members;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ILoanService _loanService;
    private readonly IMapper _mapper;

    public MembersController(IMemberService memberService, ILoanService loanService, IMapper mapper)
    {
        _memberService = memberService;
        _loanService = loanService;
        _mapper = mapper;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();

        var dto = _mapper.Map<MemberDto>(member);
        dto.ActiveLoanCount = (await _loanService.GetActiveLoansForMemberAsync(id)).Count;

        return Ok(dto);
    }
    
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
}