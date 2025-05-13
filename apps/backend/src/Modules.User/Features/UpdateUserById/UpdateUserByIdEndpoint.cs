using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Modules.User.Features.UpdateUserById
{
    public class UpdateUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/users/{id:guid}", async (
                Guid id,
                UpdateUserRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdateUserByIdCommand(
                    id,
                    request.Username,
                    request.Roles,
                    request.IsActive
                );

                var result = await sender.Send(command, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            })
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users")
            .WithName("Users_UpdateUserById")
            .Produces<UpdateUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
