using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Services.services.interfaces;

namespace LibrarySystem.Services.services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _memberRepository.GetByIdAsync(id);
    }

    public async Task<List<Member>> GetAllAsync()
    {
        return await _memberRepository.GetAllAsync();
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _memberRepository.GetByEmailAsync(email);
    }

    public async Task<Member> CreateAsync(string fullName, string email, DateTime membershipExpiryDate)
    {
        var member = new Member
        {
            FullName = fullName,
            Email = email,
            MembershipExpiryDate = membershipExpiryDate,
            OutstandingFine = 0
        };

        return await _memberRepository.AddAsync(member);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _memberRepository.ExistsByEmailAsync(email);
    }

    public async Task UpdateAsync(Member member)
    {
        await _memberRepository.UpdateAsync(member);
    }
}
