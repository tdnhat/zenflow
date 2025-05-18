using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.Workflows.CancelWorkflowRun
{
    public class CancelWorkflowRunEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflow-runs/{id}/cancel", async (Guid id, IMediator mediator) =>
            {
                var command = new CancelWorkflowRunCommand(id);
                await mediator.Send(command);
                return Results.Accepted();
            })
            .WithName("CancelWorkflowRun")
            .WithTags("WorkflowRuns")
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}