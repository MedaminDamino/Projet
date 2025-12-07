namespace API.DTO
{
    public class ServerMessage<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
