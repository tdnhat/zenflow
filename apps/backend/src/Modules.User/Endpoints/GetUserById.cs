using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Modules.User.Dtos;
using Modules.User.Features.GetUserById;

namespace Modules.User.Endpoints
{
    public class GetUserById : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/{id:guid}", async (
                [FromRoute] Guid id,
                [FromServices] ISender sender) =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await sender.Send(query);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            })
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users")
            .WithName("GetUserById")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
