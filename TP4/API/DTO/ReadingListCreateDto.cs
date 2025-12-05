namespace API.DTO
{
    public class ReadingListCreateDto
    {
        public string? ApplicationUserId { get; set; }  // Changed from int
        public int BookId { get; set; }
        public string Status { get; set; } = "NotStarted";
    }
}