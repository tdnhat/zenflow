using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.Workflows.Shared;

namespace Modules.Workflow.Features.Workflows.GetWorkflowRunStatus
{
    public class GetWorkflowRunStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflow-runs/{id}", async (Guid id, IMediator mediator) =>
            {
                var query = new GetWorkflowRunStatusQuery(id);
                var result = await mediator.Send(query);
                
                return result != null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("GetWorkflowRunStatus")
            .WithTags("WorkflowRuns")
            .Produces<WorkflowRunStatusDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}