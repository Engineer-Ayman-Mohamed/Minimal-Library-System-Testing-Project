namespace LibrarySystem.API.DTOs;

public class MemberDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MembershipExpiryDate { get; set; }
    public decimal OutstandingFine { get; set; }
    public int ActiveLoanCount { get; set; }
}
