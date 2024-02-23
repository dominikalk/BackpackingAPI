namespace Backpacking.API.Models.DTO.FriendDTOs;

public class BlockedUserDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public BlockedUserDTO(BPUser user)
    {
        Id = user.Id;
        FullName = user.FullName;
        UserName = user.UserName ?? string.Empty;
    }
}
