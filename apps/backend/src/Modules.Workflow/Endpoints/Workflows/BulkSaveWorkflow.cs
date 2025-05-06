using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.Workflows.BulkSaveWorkflow;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class BulkSaveWorkflow : ICarterModule
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
            .Produces<WorkflowDetailDto>(StatusCodes.Status200OK)
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
            .Produces<WorkflowDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }

    // Original request with all fields
    public record BulkSaveWorkflowRequest(
        string? Name,
        string? Description,
        IEnumerable<WorkflowNodeDto> Nodes,
        IEnumerable<WorkflowEdgeDto> Edges);
        
    // New request without Name and Description for the /api/workflows/{id}/save endpoint
    public record WorkflowNodesEdgesRequest(
        IEnumerable<WorkflowNodeDto> Nodes,
        IEnumerable<WorkflowEdgeDto> Edges);
}
