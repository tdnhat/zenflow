using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.Workflows.GetWorkflows
{
    public record GetWorkflowsQuery() : IRequest<List<WorkflowDto>>;
}