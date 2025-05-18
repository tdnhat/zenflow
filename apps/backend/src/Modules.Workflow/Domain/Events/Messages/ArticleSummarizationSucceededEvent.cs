namespace Modules.Workflow.Domain.Events.Messages
{
    public record ArticleSummarizationSucceededEvent(
        string Summary,
        string CorrelationId
    );

    
}