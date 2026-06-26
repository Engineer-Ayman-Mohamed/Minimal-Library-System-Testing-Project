using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.API.DTOs;

public class UpdateLoanDto
{
    [Required]
    public DateTime DueDate { get; set; }
}
