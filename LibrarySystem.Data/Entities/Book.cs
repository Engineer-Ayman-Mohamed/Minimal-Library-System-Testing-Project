using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Data.Entities;

public class Book
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ISBN must be exactly 13 digits")]
    public string ISBN { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be at least 1")]
    public int TotalCopies { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "AvailableCopies cannot be negative")]
    public int AvailableCopies { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}