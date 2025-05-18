using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Modules.Workflow.Domain.Interfaces.Core;
using MailKit.Net.Smtp;

namespace Modules.Workflow.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            IEnumerable<string> to,
            string subject,
            string body,
            bool isHtml = false,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            string from = null)
        {
            try
            {
                var message = new MimeMessage();

                // Set sender
                var senderEmail = from ?? _configuration["Email:DefaultSender"];
                message.From.Add(new MailboxAddress("ZenFlow", senderEmail));

                // Add recipients
                foreach (var recipient in to)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }

                // Add CC recipients
                if (cc != null)
                {
                    foreach (var recipient in cc)
                    {
                        message.Cc.Add(new MailboxAddress("", recipient));
                    }
                }

                // Add BCC recipients
                if (bcc != null)
                {
                    foreach (var recipient in bcc)
                    {
                        message.Bcc.Add(new MailboxAddress("", recipient));
                    }
                }

                // Set subject
                message.Subject = subject;

                // Set body
                var bodyPart = new TextPart(isHtml ? "html" : "plain")
                {
                    Text = body
                };

                message.Body = bodyPart;

                // Send email
                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"],
                    int.Parse(_configuration["Email:SmtpPort"]),
                    bool.Parse(_configuration["Email:UseSsl"]));

                // Authenticate if needed
                if (!string.IsNullOrEmpty(_configuration["Email:Username"]) &&
                    !string.IsNullOrEmpty(_configuration["Email:Password"]))
                {
                    await client.AuthenticateAsync(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipients} with subject '{Subject}'",
                    string.Join(", ", to), subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipients} with subject '{Subject}'",
                    string.Join(", ", to), subject);
                throw;
            }
        }
    }
}