using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ZenFlow.Shared.Application.Auth;

namespace ZenFlow.Shared.Infrastructure.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Username =>
            _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value;
    }
}
