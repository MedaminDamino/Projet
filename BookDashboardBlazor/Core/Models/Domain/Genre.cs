namespace BookDashboardBlazor.Core.Models.Domain;

/// <summary>
/// Represents a genre entity in the system.
/// </summary>
public class Genre
{
    public int GenreId { get; set; }
    public string GenreName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
