using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.Workflows.ValidateWorkflow;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class ValidateWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/{workflowId}/validate", async (
                Guid workflowId,
                ISender sender) =>
            {
                var command = new ValidateWorkflowCommand(workflowId);
                var result = await sender.Send(command);

                return Results.Ok(result);
            })
            .WithTags("Workflows")
            .WithName("Workflows_ValidateWorkflow")
            .Produces<ValidateWorkflowResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}