using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.GetWorkflows
{
    public record GetWorkflowsQuery() : IRequest<IEnumerable<WorkflowDto>>;
}