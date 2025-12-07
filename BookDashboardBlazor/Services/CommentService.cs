using System.Net;
using System.Net.Http;
using System.Text.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class CommentService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CommentService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Api");
    }

    public async Task<List<UserComment>> GetUserCommentsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/Comment/user");
            var raw = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new List<UserComment>();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<UserComment>();
            }

            try
            {
                var serverMessage = JsonSerializer.Deserialize<ServerMessage<List<UserComment>>>(raw, _jsonOptions);
                return serverMessage?.Data ?? new List<UserComment>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[CommentService] Error deserializing user comments: {ex.Message}");
                return new List<UserComment>();
            }
        }
        catch (Exception ex)
        {
            return new List<UserComment>();
        }
    }
}
