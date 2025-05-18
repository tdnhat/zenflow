using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;
using System.Text;
using System.Text.Json;

namespace Modules.Workflow.Infrastructure.Services.Email
{
    /// <summary>
    /// An email service implementation that saves emails to files instead of sending them.
    /// This is useful for development and testing when you don't want to actually send emails.
    /// </summary>
    public class FileEmailService : IEmailService
    {
        private readonly ILogger<FileEmailService> _logger;
        private readonly string _emailDirectory;

        public FileEmailService(ILogger<FileEmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _emailDirectory = configuration["Email:FileDirectory"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emails");
            
            // Ensure the directory exists
            if (!Directory.Exists(_emailDirectory))
            {
                Directory.CreateDirectory(_emailDirectory);
            }
        }

        public Task SendEmailAsync(
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
                var emailData = new
                {
                    To = to.ToList(),
                    Cc = cc?.ToList() ?? new List<string>(),
                    Bcc = bcc?.ToList() ?? new List<string>(),
                    From = from ?? "no-reply@zenflow.dev",
                    Subject = subject,
                    Body = body,
                    IsHtml = isHtml,
                    SentAt = DateTime.UtcNow
                };

                // Create a unique filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6);
                var filename = $"email_{timestamp}_{randomPart}.json";
                var filePath = Path.Combine(_emailDirectory, filename);

                // Write email data to file
                var json = JsonSerializer.Serialize(emailData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);

                // Also write a human-readable version for easier inspection
                var textFilePath = Path.ChangeExtension(filePath, ".txt");
                var sb = new StringBuilder();
                sb.AppendLine($"From: {emailData.From}");
                sb.AppendLine($"To: {string.Join(", ", emailData.To)}");
                
                if (emailData.Cc.Any())
                    sb.AppendLine($"Cc: {string.Join(", ", emailData.Cc)}");
                
                if (emailData.Bcc.Any())
                    sb.AppendLine($"Bcc: {string.Join(", ", emailData.Bcc)}");
                
                sb.AppendLine($"Subject: {emailData.Subject}");
                sb.AppendLine($"Date: {emailData.SentAt}");
                sb.AppendLine($"Content-Type: {(emailData.IsHtml ? "text/html" : "text/plain")}");
                sb.AppendLine();
                sb.AppendLine(body);
                
                File.WriteAllText(textFilePath, sb.ToString());

                _logger.LogInformation("Email saved to file at {FilePath}", filePath);
                _logger.LogInformation("Email would have been sent to {Recipients} with subject '{Subject}'",
                    string.Join(", ", to), subject);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email to file. Recipients: {Recipients}, Subject: '{Subject}'",
                    string.Join(", ", to), subject);
                throw;
            }
        }
    }
} 