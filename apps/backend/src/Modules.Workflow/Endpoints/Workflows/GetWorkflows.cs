using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.Workflows.GetWorkflows;
using ZenFlow.Shared.Application.Models;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class GetWorkflows : ICarterModule
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
            .Produces<List<WorkflowDto>>(StatusCodes.Status200OK);
        }
    }
}