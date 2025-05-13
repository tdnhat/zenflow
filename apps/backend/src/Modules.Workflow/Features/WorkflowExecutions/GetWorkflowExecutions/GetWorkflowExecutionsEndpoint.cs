using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.WorkflowExecutions.Common;

namespace Modules.Workflow.Features.WorkflowExecutions.GetWorkflowExecutions
{
    public class GetWorkflowExecutionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/{id}/executions", async (string id, ISender sender) =>
            {
                var query = new GetWorkflowExecutionsQuery(id);
                var result = await sender.Send(query);

                if (result == null || result.Count == 0)
                {
                    return Results.NotFound(new { Message = "No executions found for the specified workflow." });
                }
                // Add pagination headers for better client experience
                // Consider creating a helper for this
                // context.Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
                // context.Response.Headers.Add("X-Skip", result.Skip.ToString());
                // context.Response.Headers.Add("X-Take", result.Take.ToString());

                return Results.Ok(result);
            })
            // .RequireAuthorization() // Add authorization as needed
            .WithTags("Workflows")
            .WithName("Workflows_GetExecutions")
            .Produces<List<WorkflowExecutionResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound); // If workflow ID itself doesn't exist (though current handler doesn't check this)
        }
    }
}
