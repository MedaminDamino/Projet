namespace API.Models
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;

        // REQUIRED FOR EF RELATIONSHIP !!!
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
