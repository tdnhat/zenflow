using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowExecutions.RunWorkflow;

namespace Modules.Workflow.Endpoints.WorkflowExecutions
{
    public class RunWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{id}/run", async (string id, RunWorkflowCommand command, ISender sender) =>
            {
                // Set the workflow ID from the route parameter
                command = command with { WorkflowId = id };

                var result = await sender.Send(command);

                if (!result.Success)
                {
                    return Results.BadRequest(new { Error = result.Message });
                }

                return Results.Ok(result);
            })
            // .RequireAuthorization() // Commented out for testing purposes
            .WithTags("Workflows")
            .WithName("Workflows_RunWorkflow")
            .Produces<RunWorkflowResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}