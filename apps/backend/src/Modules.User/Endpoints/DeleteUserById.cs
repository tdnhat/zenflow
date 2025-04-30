using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.User.Features.DeleteUserById;

namespace Modules.User.Endpoints
{
    internal class DeleteUserById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new DeleteUserByIdCommand(id);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            })
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users")
            .WithName("DeleteUserById")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
