// File: Services/MockEmailSender.cs

using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace SubApp1.Services
{
    public class MockEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Simulate sending an email (or log it)
            Console.WriteLine($"Mock Email Sent to {email}: {subject}");
            return Task.CompletedTask;
        }
    }
}
