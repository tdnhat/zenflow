using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowExecutions.StopSampleBrowserWorkflow;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class StopSampleBrowserWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/sample-browser/stop", async (StopSampleBrowserWorkflowCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);

                if (!result.Success)
                {
                    return Results.BadRequest(new { Error = result.Message });
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_StopSampleBrowserWorkflow")
            .Produces<StopSampleBrowserWorkflowResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
} 