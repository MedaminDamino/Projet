using System.Net.Http.Json;
using BookDashboardBlazor.Core.Models.Domain;
using BookDashboardBlazor.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookDashboardBlazor.Core.Services.Api;

public class AuthorApiService : IAuthorService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthorApiService> _logger;
    private const string ApiEndpoint = "api/Author";

    public AuthorApiService(HttpClient httpClient, ILogger<AuthorApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Author>> GetAllAuthorsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Author>>(ApiEndpoint) ?? new List<Author>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching authors");
            return new List<Author>();
        }
    }

    public async Task<Author?> GetAuthorByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Author>($"{ApiEndpoint}/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching author {AuthorId}", id);
            return null;
        }
    }

    public async Task<Author> CreateAuthorAsync(Author author)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, author);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Author>() ?? author;
    }

    public async Task<Author> UpdateAuthorAsync(Author author)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{author.AuthorId}", author);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Author>() ?? author;
    }

    public async Task DeleteAuthorAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
