using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class ResendConfirmationEmailDTO
{
    [Required]
    public required string Email { get; set; }
}
