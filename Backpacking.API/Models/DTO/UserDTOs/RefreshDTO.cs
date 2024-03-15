using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.UserDTOs;

public class RefreshDTO
{
    [Required]
    public required string RefreshToken { get; set; }
}
