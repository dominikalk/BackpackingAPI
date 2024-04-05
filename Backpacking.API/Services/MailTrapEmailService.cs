using Backpacking.API.Models;
using Backpacking.API.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Backpacking.API.Services;

public class MailTrapEmailService<TUser> : IEmailService<TUser> where TUser : BPUser
{
    private readonly string? _username;
    private readonly string? _password;

    public MailTrapEmailService(IConfiguration configuration)
    {
        _username = configuration["MailTrap:Username"];
        _password = configuration["MailTrap:Password"];
    }

    /// <summary>
    /// Given a user, an email, and a confirmation link, will send an email
    /// to the user with the confirmation link to confirm their email.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="email">The email</param>
    /// <param name="confirmationLink">The confirmation link</param>
    /// <returns></returns>
    public void SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress("Backpacking", "no-reply@backpacking.com"));
        message.To.Add(new MailboxAddress(user.FullName, email));
        message.Subject = "Backpacking Confirm Email Link";

        message.Body = new TextPart("plain")
        {
            Text = $"This is your Backpacking confirm email link: {confirmationLink}"
        };

        SendEmailAsync(message);
    }

    /// <summary>
    /// Given a user, an email, and a reset code, will send an email
    /// to the user with the reset code to reset their password.
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="email">The email</param>
    /// <param name="resetCode">The reset code</param>
    public void SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        MimeMessage message = new MimeMessage();
        message.From.Add(new MailboxAddress("Backpacking", "no-reply@backpacking.com"));
        message.To.Add(new MailboxAddress(user.FullName, email));
        message.Subject = "Backpacking Password Reset Code";

        message.Body = new TextPart("plain")
        {
            Text = $"This is your Backpacking password reset code: {resetCode}"
        };

        SendEmailAsync(message);
    }

    /// <summary>
    /// Given a message, will sent the email
    /// </summary>
    /// <param name="message">The message to send</param>
    private void SendEmailAsync(MimeMessage message)
    {
        try
        {
            SmtpClient client = new SmtpClient();
            client.Connect("sandbox.smtp.mailtrap.io", 465, false);
            client.Authenticate(_username, _password);
            client.Send(message);
            client.Disconnect(true);
        }
        catch
        {
            throw new Exception("Failed to send email.");
        }
    }
}
