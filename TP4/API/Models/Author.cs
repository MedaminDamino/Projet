using API.Models;

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = null!;
    public string? Bio { get; set; }

    public ICollection<Book>? Books { get; set; }
}
