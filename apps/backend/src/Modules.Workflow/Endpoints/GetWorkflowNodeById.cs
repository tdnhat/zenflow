using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.WorkflowNodes.GetNodeById;

namespace Modules.Workflow.Endpoints
{
    public class GetWorkflowNodeById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/{workflowId}/nodes/{nodeId}", async (
                Guid workflowId, 
                Guid nodeId, 
                ISender sender) =>
            {
                var query = new GetWorkflowNodeByIdQuery(workflowId, nodeId);
                var result = await sender.Send(query);
                
                if (result == null)
                {
                    return Results.NotFound();
                }
                
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflow Nodes")
            .WithName("WorkflowNodes_GetNodeById")
            .Produces<WorkflowNodeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}