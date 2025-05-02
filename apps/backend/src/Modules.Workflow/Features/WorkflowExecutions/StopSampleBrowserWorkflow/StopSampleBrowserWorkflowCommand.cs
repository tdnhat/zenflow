using MediatR;

namespace Modules.Workflow.Features.WorkflowExecutions.StopSampleBrowserWorkflow
{
    public record StopSampleBrowserWorkflowCommand(string WorkflowInstanceId) : IRequest<StopSampleBrowserWorkflowResult>;
} 