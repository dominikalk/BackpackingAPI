using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class UpdateEmailDTO
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; } = string.Empty;
}
