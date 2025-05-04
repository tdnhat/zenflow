using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Dtos;
using Modules.Workflow.Features.Workflows.BulkSaveWorkflow;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class BulkSaveWorkflow : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/workflows/bulk-save", async (BulkSaveWorkflowCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .RequireAuthorization()
            .WithTags("Workflows")
            .WithName("Workflows_BulkSaveWorkflow")
            .Produces<WorkflowDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
