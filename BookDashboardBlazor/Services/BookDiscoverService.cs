using System.Net.Http.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class BookDiscoverService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "api";

    public BookDiscoverService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BookDiscover>?> GetBooksAsync(
        int page = 1,
        int pageSize = 20,
        string? query = null,
        List<int>? genres = null,
        int minRating = 0,
        string sort = "trending")
    {
        try
        {
            var queryParams = new Dictionary<string, string>
            {
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["sort"] = sort
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                queryParams["query"] = query;
            }

            if (minRating > 0)
            {
                queryParams["minRating"] = minRating.ToString();
            }

            if (genres?.Any() == true)
            {
                queryParams["genres"] = string.Join(",", genres);
            }

            var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{BaseUrl}/books/discover?{queryString}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<BookDiscover>>();
            }

            return new List<BookDiscover>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching books: {ex.Message}");
            return new List<BookDiscover>();
        }
    }

    public async Task<List<Genre>?> GetGenresAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Genre>>($"{BaseUrl}/genres");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching genres: {ex.Message}");
            return new List<Genre>();
        }
    }

    public async Task<List<Author>?> GetAuthorsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Author>>($"{BaseUrl}/authors");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching authors: {ex.Message}");
            return new List<Author>();
        }
    }
}