namespace Modules.Workflow.Domain.Interfaces.Core
{
    public interface IEmailService
    {
        Task SendEmailAsync(
            IEnumerable<string> to,
            string subject,
            string body,
            bool isHtml = false,
            IEnumerable<string> cc = null,
            IEnumerable<string> bcc = null,
            string from = null);
    }
} 