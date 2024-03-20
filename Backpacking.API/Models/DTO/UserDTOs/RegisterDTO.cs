using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class RegisterDTO
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public required string Email { get; set; } = string.Empty;

    [Required]
    public required string UserName { get; set; } = string.Empty;

    [Required]
    public required string Password { get; set; } = string.Empty;
}
