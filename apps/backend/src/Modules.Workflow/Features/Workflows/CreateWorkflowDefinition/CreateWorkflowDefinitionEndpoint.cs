using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.Workflows.CreateWorkflowDefinition
{
    public class CreateWorkflowDefinitionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows", async (CreateWorkflowDefinitionCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return Results.Created($"/api/workflows/{result}", new { id = result });
            })
            .WithName("CreateWorkflow")
            .WithTags("Workflows")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
        }
    }
}