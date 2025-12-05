using BookDashboardBlazor.Core.Models.Domain;

namespace BookDashboardBlazor.Core.Services.Interfaces;

/// <summary>
/// Service contract for author-related operations.
/// </summary>
public interface IAuthorService
{
    Task<List<Author>> GetAllAuthorsAsync();
    Task<Author?> GetAuthorByIdAsync(int id);
    Task<Author> CreateAuthorAsync(Author author);
    Task<Author> UpdateAuthorAsync(Author author);
    Task DeleteAuthorAsync(int id);
}
