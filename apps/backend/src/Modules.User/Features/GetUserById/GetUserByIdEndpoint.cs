using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Modules.User.Features.GetUserById
{
    public class GetUserByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users/{id:guid}", async (
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
            .WithName("Users_GetUserById")
            .Produces<GetUserByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
