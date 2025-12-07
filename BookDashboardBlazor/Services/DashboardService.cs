using System.Net.Http.Json;
using System.Text.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class DashboardService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Get dashboard summary with aggregated statistics from all entities
    /// </summary>
    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        try
        {
            // Fetch all data in parallel for better performance
            var booksTask = _http.GetFromJsonAsync<List<Book>>("api/Books");
            var authorsTask = _http.GetFromJsonAsync<List<Author>>("api/Author");
            var genresTask = _http.GetFromJsonAsync<List<Genre>>("api/Genre");
            var reviewsTask = FetchListAsync<Review>("api/Review", "reviews");
            var goalsTask = FetchListAsync<ReadingGoal>("api/ReadingGoal", "reading goals");
            var readingListsTask = FetchListAsync<ReadingList>("api/ReadingList", "reading lists");
            var commentsTask = FetchListAsync<Comment>("api/Comment", "comments");

            await Task.WhenAll(booksTask, authorsTask, genresTask, reviewsTask, goalsTask, readingListsTask, commentsTask);

            var books = booksTask.Result ?? new();
            var authors = authorsTask.Result ?? new();
            var genres = genresTask.Result ?? new();
            var reviews = reviewsTask.Result ?? new();
            var goals = goalsTask.Result ?? new();
            var readingLists = readingListsTask.Result ?? new();
            var comments = commentsTask.Result ?? new();

            var summary = new DashboardSummary
            {
                TotalBooks = books.Count,
                TotalAuthors = authors.Count,
                TotalGenres = genres.Count,
                TotalReviews = reviews.Count,
                TotalGoals = goals.Count,
                TotalReadingLists = readingLists.Count,
                TotalComments = comments.Count
            };

            return summary;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to load dashboard data from API", ex);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<List<T>> FetchListAsync<T>(string endpoint, string entityName)
    {
        var response = await _http.GetAsync(endpoint);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return new List<T>();

            throw new HttpRequestException($"Failed to load {entityName}: {response.StatusCode}");
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<T>();
        }

        var wrapped = TryDeserialize<ServerMessage<List<T>>>(raw);
        if (wrapped?.Data != null)
        {
            return wrapped.Data;
        }

        var apiResponse = TryDeserialize<ApiResponse<List<T>>>(raw);
        if (apiResponse?.Data != null)
        {
            return apiResponse.Data;
        }

        var directList = TryDeserialize<List<T>>(raw);
        if (directList != null)
        {
            return directList;
        }

        return new List<T>();
    }

    private T? TryDeserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            return default;
        }
    }
}

public class DashboardSummary
{
    public int TotalBooks { get; set; }
    public int TotalAuthors { get; set; }
    public int TotalGenres { get; set; }
    public int TotalReviews { get; set; }
    public int TotalGoals { get; set; }
    public int TotalReadingLists { get; set; }
    public int TotalComments { get; set; }
}
