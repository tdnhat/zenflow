using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.ArchiveWorkflow;

namespace Modules.Workflow.Endpoints
{
    public class ArchiveWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/workflows/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new ArchiveWorkflowCommand(id));
                
                if (result == null)
                {
                    return Results.NotFound();
                }
                
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_ArchiveWorkflow")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}