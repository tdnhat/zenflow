using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Interfaces.Core;
using Modules.Workflow.Features.Workflows.Shared;

namespace Modules.Workflow.Features.Workflows.GetWorkflowRunStatus
{
    public class GetWorkflowRunStatusHandler : IRequestHandler<GetWorkflowRunStatusQuery, WorkflowRunStatusDto?>
    {
        private readonly IWorkflowInstanceRepository _instanceRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<GetWorkflowRunStatusHandler> _logger;

        public GetWorkflowRunStatusHandler(
            IWorkflowInstanceRepository instanceRepository,
            IWorkflowRepository workflowRepository,
            ILogger<GetWorkflowRunStatusHandler> logger)
        {
            _instanceRepository = instanceRepository;
            _workflowRepository = workflowRepository;
            _logger = logger;
        }

        public async Task<WorkflowRunStatusDto?> Handle(GetWorkflowRunStatusQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting status for workflow run {WorkflowRunId}", request.WorkflowRunId);

            // Get the workflow execution context
            var context = await _instanceRepository.GetByIdAsync(request.WorkflowRunId, cancellationToken);
            if (context == null)
            {
                _logger.LogWarning("Workflow run with ID {WorkflowRunId} not found", request.WorkflowRunId);
                return null;
            }

            // Get the workflow definition
            var workflowDefinition = await _workflowRepository.GetByIdAsync(context.WorkflowDefinitionId, cancellationToken);
            if (workflowDefinition == null)
            {
                _logger.LogWarning("Workflow definition with ID {WorkflowId} not found for run {WorkflowRunId}",
                    context.WorkflowDefinitionId, request.WorkflowRunId);
                return null;
            }

            // Map to DTO
            var result = new WorkflowRunStatusDto
            {
                WorkflowRunId = context.WorkflowInstanceId,
                WorkflowId = context.WorkflowDefinitionId,
                WorkflowName = workflowDefinition.Name,
                Status = context.Status.ToString(),
                StartedAt = context.StartedAt,
                CompletedAt = context.CompletedAt,
                Nodes = context.NodeExecutions.Values.Select(n => new NodeStatusDto
                {                    NodeId = n.NodeId,
                    Name = workflowDefinition.Nodes.FirstOrDefault(node => node.Id.ToString() == n.NodeId)?.Name ?? n.NodeId,
                    ActivityType = n.ActivityType,
                    Status = n.Status.ToString(),
                    StartedAt = n.StartedAt,
                    CompletedAt = n.CompletedAt,
                    DurationMs = n.CompletedAt.HasValue && n.StartedAt.HasValue
                        ? (long)(n.CompletedAt.Value - n.StartedAt.Value).TotalMilliseconds
                        : null,
                    Error = n.Error,
                    Logs = n.Logs
                }).ToList()
            };

            return result;
        }
    }
}