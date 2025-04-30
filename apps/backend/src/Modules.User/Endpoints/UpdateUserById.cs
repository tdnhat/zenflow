using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.User.Dtos;
using Modules.User.Features.UpdateUserById;

namespace Modules.User.Endpoints
{
    public class UpdateUserById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/{id:guid}", async (
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
            .WithName("UpdateUserById")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
