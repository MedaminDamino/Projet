namespace API.DTO
{
    public class ReadingListUpdateDto
    {
        public string? ApplicationUserId { get; set; }  // Changed from int
        public int BookId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}