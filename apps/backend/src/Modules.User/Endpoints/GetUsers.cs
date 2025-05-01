using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.User.Dtos;
using Modules.User.Features.GetUsers;

namespace Modules.User.Endpoints
{
    public class GetUsers : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users/", async (
                ISender sender) =>
            {
                var result = await sender.Send(new GetUsersQuery());
                return Results.Ok(result);
            })
            .Produces<List<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithTags("Users")
            .WithName("Users_GetUsers")
            .RequireAuthorization("AdminPolicy");
        }
    }
}
