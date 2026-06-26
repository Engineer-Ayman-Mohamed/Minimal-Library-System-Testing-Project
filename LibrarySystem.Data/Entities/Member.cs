using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Data.Entities;

public class Member
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime MembershipExpiryDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OutstandingFine { get; set; } = 0;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}