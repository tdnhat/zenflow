using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.Workflow.Features.Workflows.DeleteWorkflowDefinition
{
    public class DeleteWorkflowDefinitionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/workflows/{id}", async (Guid id, IMediator mediator) =>
            {
                var command = new DeleteWorkflowDefinitionCommand(id);
                await mediator.Send(command);
                return Results.NoContent();
            })
            .WithName("DeleteWorkflow")
            .WithTags("Workflows")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}