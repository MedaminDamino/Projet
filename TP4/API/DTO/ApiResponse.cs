namespace API.DTO
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}
