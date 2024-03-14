using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Backpacking.API.Hubs;

[Authorize]
public class ChatHub : Hub { }
