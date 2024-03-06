using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class UpdateProfileDTO
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Bio { get; set; }
}
