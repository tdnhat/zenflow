using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.CreateWorkflow
{
    public record CreateWorkflowCommand(string Name, string Description) : IRequest<WorkflowResponse>;
}
