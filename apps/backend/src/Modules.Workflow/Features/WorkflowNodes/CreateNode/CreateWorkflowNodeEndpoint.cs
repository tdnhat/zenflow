using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.WorkflowNodes.CreateNode
{
    public class CreateWorkflowNodeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{workflowId:guid}/nodes", async (
                [FromRoute] Guid workflowId,
                [FromBody] CreateWorkflowNodeRequest request,
                [FromServices] ISender sender) =>
            {
                var command = new CreateWorkflowNodeCommand(
                    workflowId,
                    request.NodeType,
                    request.NodeKind,
                    request.X,
                    request.Y,
                    request.Label,
                    request.ConfigJson
                );

                var result = await sender.Send(command);
                return Results.Created($"/api/workflows/{workflowId}/nodes/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithTags("Workflow Nodes")
            .WithName("WorkflowNodes_CreateNode")
            .Produces<CreateWorkflowNodeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}