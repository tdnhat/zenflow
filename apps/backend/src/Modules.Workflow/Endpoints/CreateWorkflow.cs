using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.CreateWorkflow;

namespace Modules.Workflow.Endpoints
{
    public class CreateWorkflow : ICarterModule
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
            .Produces<WorkflowDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
        }
    }
}
