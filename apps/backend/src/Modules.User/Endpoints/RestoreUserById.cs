using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.User.Features.RestoreUserById;

namespace Modules.User.Endpoints
{
    public class RestoreUserById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/{id:guid}/restore", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RestoreUserByIdCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization("AdminPolicy")
            .WithTags("User")
            .WithName("RestoreUserById")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
