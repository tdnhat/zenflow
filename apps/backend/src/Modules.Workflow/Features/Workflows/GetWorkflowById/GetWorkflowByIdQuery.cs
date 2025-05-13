using MediatR;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public record GetWorkflowByIdQuery(Guid Id) : IRequest<WorkflowDetailResponse?>;
}