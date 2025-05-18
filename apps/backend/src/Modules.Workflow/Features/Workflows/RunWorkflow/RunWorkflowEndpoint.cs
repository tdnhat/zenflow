using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.Workflows.RunWorkflow
{
    public class RunWorkflowEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{id}/run", async (
                Guid id,
                RunWorkflowCommand command,
                IMediator mediator) =>
            {
                if (id != command.WorkflowId)
                    return Results.BadRequest("ID in route must match ID in body");
                    
                var result = await mediator.Send(command);
                return Results.Accepted($"/api/workflow-runs/{result}", new { workflowRunId = result });
            })
            .WithName("RunWorkflow")
            .WithTags("Workflows")
            .Produces<Guid>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
        }
    }
}