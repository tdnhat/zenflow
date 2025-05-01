using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.GetWorkflows;

namespace Modules.Workflow.Endpoints
{
    public class GetWorkflows : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/", async (ISender sender) =>
            {
                var result = await sender.Send(new GetWorkflowsQuery());
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_GetWorkflows")
            .Produces<IEnumerable<WorkflowDto>>(StatusCodes.Status200OK);
        }
    }
}