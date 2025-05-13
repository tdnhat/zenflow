using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.Workflows.Common;

namespace Modules.Workflow.Features.Workflows.CreateWorkflow
{
    public class CreateWorkflowEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/", async (CreateWorkflowCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/{result.Id}", result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_CreateWorkflow")
            .Produces<WorkflowResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
        }
    }
}
