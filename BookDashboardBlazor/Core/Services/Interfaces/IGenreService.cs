using BookDashboardBlazor.Core.Models.Domain;

namespace BookDashboardBlazor.Core.Services.Interfaces;

/// <summary>
/// Service contract for genre-related operations.
/// </summary>
public interface IGenreService
{
    Task<List<Genre>> GetAllGenresAsync();
    Task<Genre?> GetGenreByIdAsync(int id);
    Task<Genre> CreateGenreAsync(Genre genre);
    Task<Genre> UpdateGenreAsync(Genre genre);
    Task DeleteGenreAsync(int id);
}
