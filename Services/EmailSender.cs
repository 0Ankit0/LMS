using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using LMS.Data;

namespace LMS.Services
{
    public class IdentityEmailSender : IEmailSender<User>
    {
        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        {
            Console.WriteLine($"Sending confirmation link to {email}: {confirmationLink}");
            return Task.CompletedTask;
        }
        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            Console.WriteLine($"Sending password reset link to {email}: {resetLink}");
            return Task.CompletedTask;
        }
        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            Console.WriteLine($"Sending password reset code to {email}: {resetCode}");
            return Task.CompletedTask;
        }
    }
    public class GeneralEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"Sending email to {email} with subject '{subject}' and message: {htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
