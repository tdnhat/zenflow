﻿using MediatR;

namespace Modules.User.Features.DeleteUserById
{
    public record DeleteUserByIdCommand(Guid Id, bool Permanent = false) : IRequest;
}
