using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.GetWorkflows
{
    public class GetWorkflowsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/", async (
                ISender sender) =>
            {
                var result = await sender.Send(new GetWorkflowsQuery());
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_GetWorkflows")
            .Produces<List<WorkflowResponse>>(StatusCodes.Status200OK);
        }
    }
}