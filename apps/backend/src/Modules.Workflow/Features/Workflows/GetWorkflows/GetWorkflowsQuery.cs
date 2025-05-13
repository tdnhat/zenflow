using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.GetWorkflows
{
    public record GetWorkflowsQuery() : IRequest<List<WorkflowResponse>>;
}