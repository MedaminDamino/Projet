namespace BookDashboardBlazor.Core.Models.Domain;

/// <summary>
/// Represents an author entity in the system.
/// </summary>
public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}
