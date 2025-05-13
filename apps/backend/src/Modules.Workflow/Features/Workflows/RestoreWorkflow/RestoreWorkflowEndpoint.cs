using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.Workflows.RestoreWorkflow
{
    public class RestoreWorkflowEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{id}/restore", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new RestoreWorkflowCommand(id));

                if (result == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_RestoreWorkflow")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}