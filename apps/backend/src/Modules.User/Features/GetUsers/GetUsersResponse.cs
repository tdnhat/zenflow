namespace Modules.User.Features.GetUsers
{
    public record GetUsersResponse(Guid Id, string Username, string Email);
    //public record GetUsersResponse(IEnumerable<UserItem> Users, int TotalCount, int Page, int PageSize, int TotalPages, bool HasNextPage, bool HasPreviousPage);
} 