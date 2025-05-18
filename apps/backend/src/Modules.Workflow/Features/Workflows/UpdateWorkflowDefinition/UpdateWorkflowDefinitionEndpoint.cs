using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.Workflows.UpdateWorkflowDefinition
{
    public class UpdateWorkflowDefinitionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/workflows/{id}", async (
                Guid id,
                UpdateWorkflowDefinitionCommand command,
                IMediator mediator) =>
            {
                if (id != command.WorkflowId)
                    return Results.BadRequest("ID in route must match ID in body");

                var result = await mediator.Send(command);
                return Results.Ok(new { id = result });
            })
            .WithName("UpdateWorkflow")
            .WithTags("Workflows")
            .Produces<Guid>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();
        }
    }
}