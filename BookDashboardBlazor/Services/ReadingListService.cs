using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class ReadingListService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/ReadingList";
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ReadingListService(HttpClient http)
    {
        _http = http;
    }

    // ============================================================================
    // NEW METHODS FOR DISCOVER PAGE
    // ============================================================================

    /// <summary>
    /// Get current authenticated user's reading list as a HashSet of bookIds
    /// </summary>
    public async Task<HashSet<int>> GetUserList()
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/user");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new HashSet<int>();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new HashSet<int>();
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReadingListDto>>>();
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                var bookIds = apiResponse.Data.Select(item => item.BookId).ToHashSet();
                return bookIds;
            }

            return new HashSet<int>();
        }
        catch (Exception ex)
        {
            return new HashSet<int>();
        }
    }

    /// <summary>
    /// Returns the detailed reading list for the authenticated user, including book metadata.
    /// </summary>
    public async Task<List<ReadingList>> GetCurrentUserReadingListAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/user");
            var raw = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new List<ReadingList>();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<ReadingList>();
            }

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ReadingList>>>(raw, _jsonOptions);
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                return apiResponse.Data;
            }

            return new List<ReadingList>();
        }
        catch (Exception ex)
        {
            return new List<ReadingList>();
        }
    }

    /// <summary>
    /// Add book to reading list (for Discover page)
    /// </summary>
    public async Task<bool> AddToList(int bookId)
    {
        try
        {
            var dto = new { BookId = bookId };
            var response = await _http.PostAsJsonAsync(BaseUrl, dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
            return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Remove book from reading list by bookId (for Discover page)
    /// </summary>
    public async Task<bool> RemoveFromList(int bookId)
    {
        try
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/{bookId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
            return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    // ============================================================================
    // LEGACY METHODS (for ReadingListPage.razor admin functionality)
    // ============================================================================

    public async Task<List<ReadingList>> GetAllAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ReadingList>>(BaseUrl) ?? new List<ReadingList>();
        }
        catch (Exception ex)
        {
            return new List<ReadingList>();
        }
    }

    public async Task<ReadingList?> GetByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ReadingList>($"{BaseUrl}/id/{id}");
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(ReadingList readingList)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(BaseUrl, readingList);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(int id, ReadingList readingList)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"{BaseUrl}/{id}", readingList);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/id/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> AddToReadingListAsync(int bookId)
    {
        try
        {
            var readingListItem = new ReadingList
            {
                BookId = bookId,
                Status = "Not Started",
                AddedAt = DateTime.Now
            };

            return await CreateAsync(readingListItem);
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    // Helper DTOs
    private class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private class ReadingListDto
    {
        public int ReadingListID { get; set; }
        public int BookId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }
}
