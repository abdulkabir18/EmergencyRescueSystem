namespace Application.Common.Dtos
{
    public class PaginatedResult<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public static PaginatedResult<T> Success(List<T> data, int totalCount, int page, int size)
            => new() { Succeeded = true, Message = "Success", Data = data, TotalCount = totalCount, PageNumber = page, PageSize = size };

        public static PaginatedResult<T> Failure(string message)
            => new() { Succeeded = false, Message = message, Data = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 };
    }

}