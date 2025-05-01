using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.WorkflowEdges.CreateEdge;

namespace Modules.Workflow.Endpoints.WorkflowEdges
{
    public class CreateWorkflowEdge : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{workflowId}/edges", async (
                Guid workflowId,
                CreateEdgeRequest request,
                ISender sender) =>
            {
                var command = new CreateWorkflowEdgeCommand(
                    workflowId,
                    request.SourceNodeId,
                    request.TargetNodeId,
                    request.Label,
                    request.EdgeType,
                    request.ConditionJson,
                    request.SourceHandle,
                    request.TargetHandle
                );

                var result = await sender.Send(command);
                return Results.Created($"/api/workflows/{workflowId}/edges/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithTags("Workflow Edges")
            .WithName("WorkflowEdges_CreateEdge")
            .Produces<WorkflowEdgeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}