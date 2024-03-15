using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class ResetPasswordDTO
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string ResetCode { get; set; }
    [Required]
    public required string NewPassword { get; set; }
}
