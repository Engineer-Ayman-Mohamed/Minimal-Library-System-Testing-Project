using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.DTOs;

public class UpdateBookDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ISBN must be exactly 13 digits")]
    public string ISBN { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }
}
