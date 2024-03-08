using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.ChatDTOs;

public class CreatePrivateChatDTO
{

    [Required]
    public required Guid UserId { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Content { get; set; }
}
