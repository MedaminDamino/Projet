using System.Net.Http.Json;
using System.Linq;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class ReadingGoalService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/ReadingGoal";

    public ReadingGoalService(HttpClient http)
    {
        _http = http;
    }

    // ============================================================================
    // METHODS FOR DISCOVER PAGE (Quick Goal Creation)
    // ============================================================================

    /// <summary>
    /// Create a quick reading goal from the Discover page
    /// </summary>
    public async Task<ServerMessage<ReadingGoalViewModel>?> CreateQuickGoalAsync(int bookId, int year, int goalPercentage, int progress)
    {
        try
        {
            var dto = new
            {
                BookId = bookId,
                Year = year,
                GoalPercentage = goalPercentage,
                Progress = progress
            };

            var response = await _http.PostAsJsonAsync($"{BaseUrl}/from-book", dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new ServerMessage<ReadingGoalViewModel>
                {
                    Message = "Login to create goals",
                    Code = "UNAUTHORIZED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Try to parse error message
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    return new ServerMessage<ReadingGoalViewModel>
                    {
                        Message = errorResponse?.Message ?? "Failed to create goal",
                        Code = "ERROR"
                    };
                }
                catch
                {
                    return new ServerMessage<ReadingGoalViewModel>
                    {
                        Message = "Failed to create goal. Please try again.",
                        Code = "ERROR"
                    };
                }
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiSuccessResponse>();
            
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = apiResponse?.Message ?? "Goal created successfully",
                Data = null // We don't need to return the goal data for now
            };
        }
        catch (Exception ex)
        {
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "An error occurred while creating the goal.",
                Code = "EXCEPTION"
            };
        }
    }

    /// <summary>
    /// Get all reading goals for the current user (for checking if a book has a goal)
    /// </summary>
    public async Task<Dictionary<int, ReadingGoalViewModel>> GetUserGoalsAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{BaseUrl}/user");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new Dictionary<int, ReadingGoalViewModel>();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new Dictionary<int, ReadingGoalViewModel>();
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReadingGoalViewModel>>>();
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                var goalsDictionary = apiResponse.Data.ToDictionary(g => g.BookId);
                return goalsDictionary;
            }

            return new Dictionary<int, ReadingGoalViewModel>();
        }
        catch (Exception ex)
        {
            return new Dictionary<int, ReadingGoalViewModel>();
        }
    }

    /// <summary>
    /// Returns the list of goals for the authenticated user.
    /// </summary>
    public async Task<List<ReadingGoalViewModel>> GetUserGoalsListAsync()
    {
        var goalsDictionary = await GetUserGoalsAsync();
        return goalsDictionary.Values.ToList();
    }

    /// <summary>
    /// Returns the most recent goal for the authenticated user.
    /// </summary>
    public async Task<ReadingGoalViewModel?> GetLatestUserGoalAsync()
    {
        var goals = await GetUserGoalsListAsync();
        return goals
            .OrderByDescending(g => g.CreatedAt)
            .FirstOrDefault();
    }

    // Helper DTOs
    private class ApiSuccessResponse
    {
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    private class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================================
    // UPDATE AND DELETE METHODS
    // ============================================================================

    /// <summary>
    /// Update an existing reading goal
    /// </summary>
    public async Task<ServerMessage<ReadingGoalViewModel>?> UpdateGoalAsync(int goalId, int bookId, int year, int goalPercentage, int progress)
    {
        try
        {
            var dto = new
            {
                BookId = bookId,
                Year = year,
                GoalPercentage = goalPercentage,
                Progress = progress
            };

            var response = await _http.PutAsJsonAsync($"{BaseUrl}/{goalId}", dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new ServerMessage<ReadingGoalViewModel>
                {
                    Message = "Login to update goals",
                    Code = "UNAUTHORIZED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    return new ServerMessage<ReadingGoalViewModel>
                    {
                        Message = errorResponse?.Message ?? "Failed to update goal",
                        Code = "ERROR"
                    };
                }
                catch
                {
                    return new ServerMessage<ReadingGoalViewModel>
                    {
                        Message = "Failed to update goal. Please try again.",
                        Code = "ERROR"
                    };
                }
            }

            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "Goal updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "An error occurred while updating the goal.",
                Code = "EXCEPTION"
            };
        }
    }

    /// <summary>
    /// Delete a reading goal
    /// </summary>
    public async Task<ServerMessage<ReadingGoalViewModel>?> DeleteGoalAsync(int goalId)
    {
        try
        {
            var response = await _http.DeleteAsync($"{BaseUrl}/{goalId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new ServerMessage<ReadingGoalViewModel>
                {
                    Message = "Login to delete goals",
                    Code = "UNAUTHORIZED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    return new ServerMessage<ReadingGoalViewModel>
                    {
                        Message = errorResponse?.Message ?? "Failed to delete goal",
                        Code = "ERROR"
                    };
                }
                catch
                {
                    return new ServerMessage<ReadingGoalViewModel>
                    {
                        Message = "Failed to delete goal. Please try again.",
                        Code = "ERROR"
                    };
                }
            }

            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "Goal deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "An error occurred while deleting the goal.",
                Code = "EXCEPTION"
            };
        }
    }
}

/// <summary>
/// View model for reading goals
/// </summary>
public class ReadingGoalViewModel
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int GoalPercentage { get; set; }
    public int Progress { get; set; }
    public int BookId { get; set; }
    public string? ApplicationUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
