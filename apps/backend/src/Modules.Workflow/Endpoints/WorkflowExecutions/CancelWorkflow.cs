using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowExecutions.CancelWorkflow;

namespace Modules.Workflow.Endpoints.WorkflowExecutions
{
    public class CancelWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{id}/cancel", async (string id, CancelWorkflowCommand command, ISender sender) =>
            {
                // Set the workflow ID from the route parameter
                command = command with { WorkflowId = id };

                var result = await sender.Send(command);

                if (!result.Success)
                {
                    // Consider returning 404 if not found, 409 if already completed/failed, 400 for bad ID
                    // For simplicity, returning BadRequest for now on any failure.
                    return Results.BadRequest(new { Error = result.Message });
                }

                return Results.Ok(result);
            })
            // .RequireAuthorization() // Commented out for testing purposes
            .WithTags("Workflows")
            .WithName("Workflows_CancelWorkflow")
            .Produces<CancelWorkflowResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest) // Add other potential error codes like 404, 409
            .Produces(StatusCodes.Status404NotFound); // If workflow/execution not found
        }
    }
}
