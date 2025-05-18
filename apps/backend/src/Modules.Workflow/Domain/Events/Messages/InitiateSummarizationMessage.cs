namespace Modules.Workflow.Domain.Events.Messages
{
    public record InitiateSummarizationMessage(
        string TextToSummarize,
        int? MaxSummaryLength,
        string CorrelationId
    );
}