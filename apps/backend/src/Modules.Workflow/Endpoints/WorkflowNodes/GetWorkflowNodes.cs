using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.WorkflowNodes.GetNodes;

namespace Modules.Workflow.Endpoints.WorkflowNodes
{
    public class GetWorkflowNodes : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/{workflowId}/nodes", async (
                Guid workflowId,
                ISender sender) =>
            {
                var query = new GetWorkflowNodesQuery(workflowId);
                var result = await sender.Send(query);

                if (result == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflow Nodes")
            .WithName("WorkflowNodes_GetNodes")
            .Produces<IEnumerable<WorkflowNodeDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}