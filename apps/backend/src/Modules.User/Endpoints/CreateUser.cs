using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Modules.User.Dtos;
using Modules.User.Features.CreateUser;
using ZenFlow.Shared.Application.Auth;

namespace Modules.User.Endpoints
{
    public class CreateUser : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users/", async (
                ISender sender,
                ICurrentUserService currentUser) =>
            {
                if (currentUser.UserId is null || currentUser.Email is null)
                    return Results.Unauthorized();

                var command = new CreateUserCommand(
                    currentUser.UserId, // externalId = sub (NameIdentifier)
                    currentUser.Username ?? currentUser.Email,
                    currentUser.Email
                );

                var result = await sender.Send(command);
                return Results.Created($"/{result.Id}", result);
            })
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Users")
            .WithName("Users_CreateUser")
            .RequireAuthorization();
        }
    }
}
