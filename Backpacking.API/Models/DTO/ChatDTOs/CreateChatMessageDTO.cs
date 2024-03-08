using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.ChatDTOs;

public class CreateChatMessageDTO
{
    [Required]
    [MaxLength(1000)]
    public required string Content { get; set; }
}
