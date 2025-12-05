using System.Net.Http.Json;
using BookDashboardBlazor.Core.Models.Domain;
using BookDashboardBlazor.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BookDashboardBlazor.Core.Services.Api;

public class GenreApiService : IGenreService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GenreApiService> _logger;
    private const string ApiEndpoint = "api/Genre";

    public GenreApiService(HttpClient httpClient, ILogger<GenreApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Genre>> GetAllGenresAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Genre>>(ApiEndpoint) ?? new List<Genre>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching genres");
            return new List<Genre>();
        }
    }

    public async Task<Genre?> GetGenreByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Genre>($"{ApiEndpoint}/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching genre {GenreId}", id);
            return null;
        }
    }

    public async Task<Genre> CreateGenreAsync(Genre genre)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, genre);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Genre>() ?? genre;
    }

    public async Task<Genre> UpdateGenreAsync(Genre genre)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{genre.GenreId}", genre);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Genre>() ?? genre;
    }

    public async Task DeleteGenreAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
