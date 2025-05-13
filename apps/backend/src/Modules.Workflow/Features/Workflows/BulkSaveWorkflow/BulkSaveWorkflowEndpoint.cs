using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowEdges.UpdateEdge;
using Modules.Workflow.Features.WorkflowNodes.UpdateNode;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.BulkSaveWorkflow
{
    public class BulkSaveWorkflowEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/bulk-save", async (BulkSaveWorkflowCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_BulkSaveWorkflow")
            .Produces<WorkflowDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("/api/workflows/{id}/save", async (
                Guid id,
                WorkflowNodesEdgesRequest request,
                ISender sender) =>
            {
                var command = new BulkSaveWorkflowCommand(
                    id,
                    null, // Name is not included for updates
                    null, // Description is not included for updates
                    request.Nodes,
                    request.Edges
                );

                var result = await sender.Send(command);

                if (result == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_BulkSave")
            .Produces<WorkflowResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }

    // Original request with all fields
    public record BulkSaveWorkflowRequest(
        string? Name,
        string? Description,
        IEnumerable<WorkflowNodeResponse> Nodes,
        IEnumerable<WorkflowEdgeResponse> Edges);

    // New request without Name and Description for the /api/workflows/{id}/save endpoint
    public record WorkflowNodesEdgesRequest(
        IEnumerable<UpdateWorkflowNodeRequest> Nodes,
        IEnumerable<UpdateWorkflowEdgeRequest> Edges);
}
