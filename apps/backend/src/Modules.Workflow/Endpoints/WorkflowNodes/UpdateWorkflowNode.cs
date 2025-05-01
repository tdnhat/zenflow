using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.WorkflowNodes.UpdateNode;

namespace Modules.Workflow.Endpoints.WorkflowNodes
{
    public class UpdateWorkflowNode : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/workflows/{workflowId}/nodes/{nodeId}", async (
                Guid workflowId,
                Guid nodeId,
                UpdateNodeRequest request,
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
            .Produces<WorkflowNodeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}