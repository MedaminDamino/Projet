using System.Net.Http.Json;
using BookDashboardBlazor.Core.Models.Domain;
using BookDashboardBlazor.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookDashboardBlazor.Core.Services.Api;

/// <summary>
/// API client service for book operations.
/// </summary>
public class BookApiService : IBookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BookApiService> _logger;
    private const string ApiEndpoint = "api/Books";

    public BookApiService(HttpClient httpClient, ILogger<BookApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Book>> GetAllBooksAsync()
    {
        try
        {
            var books = await _httpClient.GetFromJsonAsync<List<Book>>(ApiEndpoint);
            return books ?? new List<Book>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all books");
            return new List<Book>();
        }
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Book>($"{ApiEndpoint}/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching book with ID {BookId}", id);
            return null;
        }
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, book);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Book>() ?? book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book");
            throw;
        }
    }

    public async Task<Book> UpdateBookAsync(Book book)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{book.BookId}", book);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Book>() ?? book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book with ID {BookId}", book.BookId);
            throw;
        }
    }

    public async Task DeleteBookAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book with ID {BookId}", id);
            throw;
        }
    }
}
