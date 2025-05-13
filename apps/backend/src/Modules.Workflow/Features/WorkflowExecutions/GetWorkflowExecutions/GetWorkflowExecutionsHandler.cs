using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.DDD.Interfaces;
using Modules.Workflow.DDD.ValueObjects;
using Modules.Workflow.Features.WorkflowExecutions.Common;

namespace Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions
{
    public class GetWorkflowExecutionsHandler : IRequestHandler<GetWorkflowExecutionsQuery, List<WorkflowExecutionResponse>>
    {
        private readonly IWorkflowExecutionRepository _executionRepository;
        private readonly ILogger<GetWorkflowExecutionsHandler> _logger;

        public GetWorkflowExecutionsHandler(IWorkflowExecutionRepository executionRepository, ILogger<GetWorkflowExecutionsHandler> logger)
        {
            _executionRepository = executionRepository;
            _logger = logger;
        }

        public async Task<List<WorkflowExecutionResponse>> Handle(GetWorkflowExecutionsQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowIdGuid))
            {
                _logger.LogWarning("Invalid Workflow ID format: {WorkflowId}", request.WorkflowId);
                return new List<WorkflowExecutionResponse>();
            }

            try
            {
                _logger.LogInformation("Fetching executions for workflow ID {WorkflowId}", workflowIdGuid);

                var executions = await _executionRepository.GetByWorkflowIdAsync(workflowIdGuid, cancellationToken);
                var totalCount = await _executionRepository.CountByWorkflowIdAsync(workflowIdGuid, cancellationToken);

                var dtos = executions.Select(e => new WorkflowExecutionResponse(
                    e.Id,
                    e.WorkflowId,
                    e.WorkflowVersion,
                    e.Status.ToStringValue(),
                    e.StartedAt,
                    e.CompletedAt
                )).ToList();

                _logger.LogInformation("Fetched {Count} executions for workflow ID {WorkflowId}", dtos.Count, workflowIdGuid);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching executions for workflow ID {WorkflowId}: {ErrorMessage}", workflowIdGuid, ex.Message);
                return new List<WorkflowExecutionResponse>();
            }
        }
    }
}
