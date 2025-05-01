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
                [AsParameters] WorkflowsFilterRequest filter,
                ISender sender) =>
            {
                var result = await sender.Send(new GetWorkflowsQuery(filter));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_GetWorkflows")
            .Produces<PaginatedResult<WorkflowDto>>(StatusCodes.Status200OK);
        }
    }
}