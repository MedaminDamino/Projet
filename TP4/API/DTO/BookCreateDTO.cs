namespace API.DTO
{
    public class BookCreateDto
    {
        public string Title { get; set; } = null!;
        public int AuthorId { get; set; }
        public int GenreId { get; set; }
        public int? PublishYear { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; } // For file uploads
    }
}