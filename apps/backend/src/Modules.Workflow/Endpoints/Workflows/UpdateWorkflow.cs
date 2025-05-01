using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.Workflows.UpdateWorkflow;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class UpdateWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/workflows/{id}", async (Guid id, UpdateWorkflowCommand command, ISender sender) =>
            {
                // Ensure the ID in the route matches the ID in the command
                if (id != command.Id)
                {
                    return Results.BadRequest("ID in the route must match ID in the request body");
                }

                var result = await sender.Send(command);

                if (result == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_UpdateWorkflow")
            .Produces<WorkflowDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem();
        }
    }
}