using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.WorkflowNodes.CreateNode;

namespace Modules.Workflow.Endpoints.WorkflowNodes
{
    public class CreateWorkflowNode : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{workflowId}/nodes", async (Guid workflowId, CreateNodeRequest request, ISender sender) =>
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
            .Produces<WorkflowNodeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}