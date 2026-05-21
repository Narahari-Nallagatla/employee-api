namespace EmployeeApi.Responses
{
    public class PagedResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public T Data { get; set; }
    }
}
