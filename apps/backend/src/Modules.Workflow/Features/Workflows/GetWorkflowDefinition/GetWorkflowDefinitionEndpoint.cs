using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Features.Workflows.Shared;

namespace Modules.Workflow.Features.Workflows.GetWorkflowById
{
    public class GetWorkflowByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/workflows/{id}", async (
                Guid id,
                IMediator mediator) =>
                {
                    var query = new GetWorkflowDefinitionQuery(id);
                    var result = await mediator.Send(query);

                    return result != null
                        ? Results.Ok(result)
                        : Results.NotFound();
                }
            )
            .WithName("GetWorkflowById")
            .WithTags("Workflows")
            .Produces<WorkflowDefinitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}