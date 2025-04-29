using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.User.Dtos;
using Modules.User.Features.GetCurrentUser;
using ZenFlow.Shared.Application.Auth;

namespace Modules.User.Endpoints
{
    public class GetCurrentUser : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/me", async (
                ISender sender,
                ICurrentUserService currentUser) =>
            {
                if (currentUser.UserId is null || currentUser.Email is null)
                    return Results.Unauthorized();

                var query = new GetCurrentUserQuery(
                    currentUser.UserId,
                    currentUser.Username ?? currentUser.Email,
                    currentUser.Email
                );

                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Users")
            .WithName("GetCurrentUser")
            .RequireAuthorization();
        }
    }
}
