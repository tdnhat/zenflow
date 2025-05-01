using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.GetWorkflowById;

namespace Modules.Workflow.Endpoints
{
    public class GetWorkflowById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetWorkflowByIdQuery(id));
                
                if (result == null)
                {
                    return Results.NotFound();
                }
                
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_GetWorkflowById")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}