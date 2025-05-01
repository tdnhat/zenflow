using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.GetWorkflowById
{
    public record GetWorkflowByIdQuery(Guid Id) : IRequest<WorkflowDetailDto?>;
}