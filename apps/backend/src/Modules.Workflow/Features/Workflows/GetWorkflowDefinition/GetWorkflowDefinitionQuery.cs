using MediatR;
using Modules.Workflow.Features.Workflows.Shared;

namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public record GetWorkflowDefinitionQuery(Guid Id) : IRequest<WorkflowDefinitionDto?>;
}