namespace ZenFlow.Shared.Application.Models
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public PaginatedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
        
        public static PaginatedResult<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
        {
            return new PaginatedResult<T>(items, totalCount, page, pageSize);
        }
    }
}