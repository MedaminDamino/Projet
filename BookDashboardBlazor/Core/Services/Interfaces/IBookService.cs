using BookDashboardBlazor.Core.Models.Domain;

namespace BookDashboardBlazor.Core.Services.Interfaces;

/// <summary>
/// Service contract for book-related operations.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Retrieves all books from the system.
    /// </summary>
    Task<List<Book>> GetAllBooksAsync();

    /// <summary>
    /// Retrieves a specific book by its ID.
    /// </summary>
    Task<Book?> GetBookByIdAsync(int id);

    /// <summary>
    /// Creates a new book in the system.
    /// </summary>
    Task<Book> CreateBookAsync(Book book);

    /// <summary>
    /// Updates an existing book.
    /// </summary>
    Task<Book> UpdateBookAsync(Book book);

    /// <summary>
    /// Deletes a book by its ID.
    /// </summary>
    Task DeleteBookAsync(int id);
}
