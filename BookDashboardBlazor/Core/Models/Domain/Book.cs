namespace BookDashboardBlazor.Core.Models.Domain;

/// <summary>
/// Represents a book entity in the system.
/// </summary>
public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int GenreId { get; set; }
    public int PublishYear { get; set; }
    public string Description { get; set; } = string.Empty;
}
