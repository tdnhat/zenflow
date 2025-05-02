using MediatR;

namespace Modules.Workflow.Features.WorkflowExecutions.RunSampleBrowserWorkflow
{
    public record RunSampleBrowserWorkflowCommand() : IRequest<RunSampleBrowserWorkflowResult>;
} 