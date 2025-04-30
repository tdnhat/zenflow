using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;  // Added for FromQuery
using Microsoft.AspNetCore.Routing;
using Modules.User.Features.DeleteUserById;
using Modules.User.Infrastructure.Exceptions;

namespace Modules.User.Endpoints
{
    internal class DeleteUserById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id:guid}", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken,
                [FromQuery] bool permanent = false) =>
            {
                try
                {
                    // Create command with permanent deletion flag
                    var command = new DeleteUserByIdCommand(id, permanent);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                }
                catch (NotFoundException)
                {
                    return Results.NotFound(new { message = $"User with ID {id} not found" });
                }
                catch (Exception ex)
                {
                    // Log exception in production
                    return Results.Problem(
                        title: "An error occurred while deleting the user",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users")
            .WithName("DeleteUserById")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
