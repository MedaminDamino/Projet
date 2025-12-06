namespace API.DTO
{
    public class BookReadDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public int? AuthorId { get; set; }
        public int? GenreId { get; set; }
        public int? PublishYear { get; set; }
        public string? Description { get; set; }    
        public string? ImageUrl { get; set; } 
    }
}