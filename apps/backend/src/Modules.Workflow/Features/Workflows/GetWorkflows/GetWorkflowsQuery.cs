using MediatR;
using Modules.Workflow.Dtos;
using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.Features.Workflows.GetWorkflows
{
    public record GetWorkflowsQuery(WorkflowsFilterRequest Filter) : IRequest<PaginatedResult<WorkflowDto>>;
}