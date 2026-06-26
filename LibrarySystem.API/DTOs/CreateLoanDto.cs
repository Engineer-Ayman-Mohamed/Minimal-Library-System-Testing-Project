using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.DTOs;

public class CreateLoanDto
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int BookId { get; set; }
}
