using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowEdges.DeleteEdge;

namespace Modules.Workflow.Endpoints.WorkflowEdges
{
    public class DeleteWorkflowEdge : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/workflows/{workflowId}/edges/{edgeId}", async (
                Guid workflowId,
                Guid edgeId,
                ISender sender) =>
            {
                var command = new DeleteWorkflowEdgeCommand(edgeId, workflowId);
                var result = await sender.Send(command);

                if (!result)
                {
                    return Results.NotFound();
                }

                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags("Workflow Edges")
            .WithName("WorkflowEdges_DeleteEdge")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}