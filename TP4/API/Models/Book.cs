namespace API.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public int AuthorId { get; set; }
        public int GenreId { get; set; }
        public int? PublishYear { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; } // Store the URL path
        public Author Author { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
        public ICollection<ReadingList> ReadingLists { get; set; } = new List<ReadingList>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}