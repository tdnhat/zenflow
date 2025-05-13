using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.User.Features.RestoreUserById
{
    public class RestoreUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/users/{id:guid}/restore", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new RestoreUserByIdCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users")
            .WithName("Users_RestoreUserById")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
