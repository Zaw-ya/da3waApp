using Microsoft.AspNetCore.Identity.UI.Services;

namespace Da3wa.WebUI.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now, we don't send emails. 
            // You can implement your own email sending logic here (e.g., using SendGrid or SMTP).
            return Task.CompletedTask;
        }
    }
}
