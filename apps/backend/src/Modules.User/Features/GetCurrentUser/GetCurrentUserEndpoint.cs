﻿using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZenFlow.Shared.Application.Auth;

namespace Modules.User.Features.GetCurrentUser
{
    public class GetCurrentUserEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users/me", async (
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
            .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags("Users")
            .WithName("Users_GetCurrentUser")
            .RequireAuthorization();
        }
    }
}
