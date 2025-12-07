using System.Net.Http.Json;
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
            Console.WriteLine($"[ReadingGoalService] Creating quick goal for book {bookId}: Year={year}, Goal%={goalPercentage}, Progress={progress}");
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
                Console.WriteLine($"[ReadingGoalService] ⚠️ Unauthorized when creating goal");
                return new ServerMessage<ReadingGoalViewModel>
                {
                    Message = "Login to create goals",
                    Code = "UNAUTHORIZED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingGoalService] ❌ Failed to create goal: {response.StatusCode} - {errorContent}");
                
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
            Console.WriteLine($"[ReadingGoalService] ✅ Successfully created goal");
            
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = apiResponse?.Message ?? "Goal created successfully",
                Data = null // We don't need to return the goal data for now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingGoalService] ❌ Exception creating goal: {ex.Message}");
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
            Console.WriteLine("[ReadingGoalService] GetUserGoals - Starting API call to /api/ReadingGoal/user");
            var response = await _http.GetAsync($"{BaseUrl}/user");
            Console.WriteLine($"[ReadingGoalService] GetUserGoals - Response status: {response.StatusCode}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("[ReadingGoalService] ⚠️ Unauthorized access to reading goals (401)");
                return new Dictionary<int, ReadingGoalViewModel>();
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingGoalService] ❌ Failed to get user goals: {response.StatusCode}");
                Console.WriteLine($"[ReadingGoalService] Error content: {errorContent}");
                return new Dictionary<int, ReadingGoalViewModel>();
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ReadingGoalViewModel>>>();
            if (apiResponse?.Success == true && apiResponse.Data != null)
            {
                var goalsDictionary = apiResponse.Data.ToDictionary(g => g.BookId);
                Console.WriteLine($"[ReadingGoalService] ✅ Successfully loaded {goalsDictionary.Count} reading goals");
                return goalsDictionary;
            }

            Console.WriteLine("[ReadingGoalService] ⚠️ API response was unsuccessful or contained no data");
            return new Dictionary<int, ReadingGoalViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingGoalService] ❌ Exception fetching user reading goals: {ex.Message}");
            return new Dictionary<int, ReadingGoalViewModel>();
        }
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
    public async Task<ServerMessage<ReadingGoalViewModel>?> UpdateGoalAsync(int goalId, int year, int goalPercentage, int progress)
    {
        try
        {
            Console.WriteLine($"[ReadingGoalService] Updating goal {goalId}: Year={year}, Goal%={goalPercentage}, Progress={progress}");
            var dto = new
            {
                Year = year,
                GoalPercentage = goalPercentage,
                Progress = progress
            };

            var response = await _http.PutAsJsonAsync($"{BaseUrl}/{goalId}", dto);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"[ReadingGoalService] ⚠️ Unauthorized when updating goal");
                return new ServerMessage<ReadingGoalViewModel>
                {
                    Message = "Login to update goals",
                    Code = "UNAUTHORIZED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingGoalService] ❌ Failed to update goal: {response.StatusCode} - {errorContent}");
                
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

            Console.WriteLine($"[ReadingGoalService] ✅ Successfully updated goal");
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "Goal updated successfully"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingGoalService] ❌ Exception updating goal: {ex.Message}");
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
            Console.WriteLine($"[ReadingGoalService] Deleting goal {goalId}");
            var response = await _http.DeleteAsync($"{BaseUrl}/{goalId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"[ReadingGoalService] ⚠️ Unauthorized when deleting goal");
                return new ServerMessage<ReadingGoalViewModel>
                {
                    Message = "Login to delete goals",
                    Code = "UNAUTHORIZED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ReadingGoalService] ❌ Failed to delete goal: {response.StatusCode} - {errorContent}");
                
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

            Console.WriteLine($"[ReadingGoalService] ✅ Successfully deleted goal");
            return new ServerMessage<ReadingGoalViewModel>
            {
                Message = "Goal deleted successfully"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReadingGoalService] ❌ Exception deleting goal: {ex.Message}");
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
