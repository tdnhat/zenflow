using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.Workflows.CreateWorkflow
{
    public record CreateWorkflowCommand(string Name, string Description) : IRequest<WorkflowDto>;
}
