namespace Modules.Workflow.Domain.Events.Messages
{
    public record ArticleSummarizationFailedEvent(
        string ErrorMessage,
        string CorrelationId
    );
}