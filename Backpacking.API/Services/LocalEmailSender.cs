using Backpacking.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Backpacking.API.Services;

public class LocalEmailSender<TUser> : IEmailSender<TUser> where TUser : BPUser
{
    public Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        return Task.CompletedTask;
    }
}
