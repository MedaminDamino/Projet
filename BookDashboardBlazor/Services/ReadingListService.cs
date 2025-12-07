using System.Net.Http.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class ReadingListService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/ReadingList";

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
            Console.WriteLine("[ReadingListService] GetUserList - Starting API call to /api/ReadingList/user");
            var response = await _http.GetAsync($"{BaseUrl}/user");
            Console.WriteLine($"[ReadingListService] GetUserList - Response status: {response.StatusCode}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("[ReadingListService] ⚠️ Unauthorized access to reading list (401)");
                return new HashSet<int>();
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingListService] ❌ Failed to get user list: {response.StatusCode}");
                Console.WriteLine($"[ReadingListService] Error content: {errorContent}");
                return new HashSet<int>();
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReadingListDto>>>();
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                var bookIds = apiResponse.Data.Select(item => item.BookId).ToHashSet();
                Console.WriteLine($"[ReadingListService] ✅ Successfully loaded {bookIds.Count} books in reading list: [{string.Join(", ", bookIds)}]");
                return bookIds;
            }

            Console.WriteLine("[ReadingListService] ⚠️ API response was unsuccessful or contained no data");
            return new HashSet<int>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingListService] ❌ Exception fetching user reading list: {ex.Message}");
            Console.WriteLine($"[ReadingListService] Stack trace: {ex.StackTrace}");
            return new HashSet<int>();
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
                Console.WriteLine($"[ReadingListService] Unauthorized when adding book {bookId}");
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingListService] Failed to add book {bookId}: {response.StatusCode} - {errorContent}");
                return false;
            }

            Console.WriteLine($"[ReadingListService] Successfully added book {bookId} to reading list");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingListService] Error adding book {bookId} to reading list: {ex.Message}");
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
                Console.WriteLine($"[ReadingListService] Unauthorized when removing book {bookId}");
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingListService] Failed to remove book {bookId}: {response.StatusCode} - {errorContent}");
                return false;
            }

            Console.WriteLine($"[ReadingListService] Successfully removed book {bookId} from reading list");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingListService] Error removing book {bookId} from reading list: {ex.Message}");
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
            Console.WriteLine($"Error fetching reading list: {ex.Message}");
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
            Console.WriteLine($"Error fetching reading list item {id}: {ex.Message}");
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
            Console.WriteLine($"Error creating reading list item: {ex.Message}");
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
            Console.WriteLine($"Error updating reading list item {id}: {ex.Message}");
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
            Console.WriteLine($"Error deleting reading list item {id}: {ex.Message}");
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
            Console.WriteLine($"Error adding book {bookId} to reading list: {ex.Message}");
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
