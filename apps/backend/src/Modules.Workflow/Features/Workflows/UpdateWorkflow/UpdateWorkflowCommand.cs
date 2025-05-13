using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.UpdateWorkflow
{
    public record UpdateWorkflowCommand(Guid Id, string Name, string Description) : IRequest<WorkflowResponse?>;
}