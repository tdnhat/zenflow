using MediatR;
using Modules.Workflow.Dtos;

namespace Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions
{
    public record GetWorkflowExecutionsQuery(string WorkflowId) : IRequest<List<WorkflowExecutionDto>>;
}
