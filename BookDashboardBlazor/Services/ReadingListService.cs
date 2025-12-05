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
            return await _http.GetFromJsonAsync<ReadingList>($"{BaseUrl}/{id}");
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
            var response = await _http.DeleteAsync($"{BaseUrl}/{id}");
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
}
