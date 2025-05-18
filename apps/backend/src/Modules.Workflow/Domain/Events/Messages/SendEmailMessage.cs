namespace Modules.Workflow.Domain.Events.Messages
{
    public record SendEmailMessage(
        IList<string> To,
        IList<string> Cc,
        IList<string> Bcc,
        string From,
        string Subject,
        string Body,
        bool IsHtml
    );
}