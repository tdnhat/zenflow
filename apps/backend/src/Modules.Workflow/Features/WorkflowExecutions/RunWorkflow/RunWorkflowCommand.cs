using MediatR;
using System.Collections.Generic;

namespace Modules.Workflow.Features.WorkflowExecutions.RunWorkflow
{
    public record RunWorkflowCommand : IRequest<RunWorkflowResult>
    {
        public string WorkflowId { get; init; } = null!;
        
        // Browser automation specific parameters
        public string? SearchTerm { get; init; }
        public bool? TakeScreenshots { get; init; }
        
        // Generic input parameters
        public Dictionary<string, object>? Input { get; init; }
    }
} 