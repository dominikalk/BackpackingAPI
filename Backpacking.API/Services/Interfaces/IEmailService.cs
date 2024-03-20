namespace Backpacking.API.Services.Interfaces;

public interface IEmailService<TUser>
{
    void SendConfirmationLinkAsync(TUser user, string email, string confirmationLink);
    void SendPasswordResetCodeAsync(TUser user, string email, string resetCode);
}
