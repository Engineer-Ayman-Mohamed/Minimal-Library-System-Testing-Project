using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.DTOs;

public class CreateMemberDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime MembershipExpiryDate { get; set; } = DateTime.Today.AddYears(1);
}
