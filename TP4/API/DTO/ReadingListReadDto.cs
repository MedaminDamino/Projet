namespace API.DTO
{
    public class ReadingListReadDto
    {
        public int ReadingListID { get; set; }
        public string? ApplicationUserId { get; set; }  // Changed from int
        public int BookId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
        public BookReadDto? Book { get; set; }
    }
}