namespace Application.Common.Dtos
{
    public class PaginatedResult<T> : Result<List<T>>
    {
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        private PaginatedResult(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Succeeded = true;
            Message = "Success";
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PaginatedResult<T> Create(List<T> data, int totalCount, int pageNumber, int pageSize)
            => new PaginatedResult<T>(data, totalCount, pageNumber, pageSize);

        public static PaginatedResult<T> Failure(string message)
            => new PaginatedResult<T>(new List<T>(), 0, 1, 10)
            {
                Succeeded = false,
                Message = message
            };
    }
}
