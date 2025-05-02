using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowExecutions.RunSampleBrowserWorkflow;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class RunSampleBrowserWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/sample-browser/run", async (ISender sender) =>
            {
                var result = await sender.Send(new RunSampleBrowserWorkflowCommand());

                if (!result.Success)
                {
                    return Results.BadRequest(new { Error = result.Message });
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_RunSampleBrowserWorkflow")
            .Produces<RunSampleBrowserWorkflowResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
} 