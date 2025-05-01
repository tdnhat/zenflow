namespace ZenFlow.Shared.Application.Models
{
    public class PaginatedRequest
    {
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;
        
        private int _pageSize = DefaultPageSize;
        private int _page = 1;
        
        public int Page 
        { 
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }
        
        public int PageSize 
        { 
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => value
            };
        }
    }
}