using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class ForgotPasswordDTO
{
    [Required]
    public required string Email { get; set; }
}
