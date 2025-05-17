using MediatR;

namespace Modules.Workflow.Features.Workflows.DeleteWorkflowDefinition
{
    public record DeleteWorkflowDefinitionCommand(Guid WorkflowId) : IRequest;
}