using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowNodes.DeleteNode;

namespace Modules.Workflow.Endpoints
{
    public class DeleteWorkflowNode : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/workflows/{workflowId}/nodes/{nodeId}", async (
                Guid workflowId, 
                Guid nodeId, 
                ISender sender) =>
            {
                var command = new DeleteWorkflowNodeCommand(nodeId, workflowId);
                var result = await sender.Send(command);
                
                if (!result)
                {
                    return Results.NotFound();
                }
                
                return Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags("Workflow Nodes")
            .WithName("WorkflowNodes_DeleteNode")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}