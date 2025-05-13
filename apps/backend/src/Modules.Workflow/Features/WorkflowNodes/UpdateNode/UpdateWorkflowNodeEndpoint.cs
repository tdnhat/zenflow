using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.WorkflowNodes.UpdateNode
{
    public class UpdateWorkflowNodeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/workflows/{workflowId}/nodes/{nodeId}", async (
                Guid workflowId,
                Guid nodeId,
                UpdateWorkflowNodeRequest request,
                ISender sender) =>
            {
                var command = new UpdateWorkflowNodeCommand(
                    nodeId,
                    workflowId,
                    request.X,
                    request.Y,
                    request.Label,
                    request.ConfigJson
                );

                var result = await sender.Send(command);

                if (result == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflow Nodes")
            .WithName("WorkflowNodes_UpdateNode")
            .Produces<WorkflowNodeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}