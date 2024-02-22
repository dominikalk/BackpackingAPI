using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class LoginDTO
{
    [Required]
    public required string UserName { get; set; } = string.Empty;

    [Required]
    public required string Password { get; set; } = string.Empty;
}
