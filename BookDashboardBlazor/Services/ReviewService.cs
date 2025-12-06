using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookDashboardBlazor.Models;

namespace BookDashboardBlazor.Services;

public class ReviewService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ReviewService(IHttpClientFactory httpClientFactory)
    {
        // Explicitly use the "Api" named client which has AuthHeaderHandler configured
        _http = httpClientFactory.CreateClient("Api");
    }

    public async Task<ReviewServiceResult<UserRatingDto>> GetUserRatingAsync(int bookId)
    {
        try
        {
            var response = await _http.GetAsync($"api/Review/user-rating?bookId={bookId}");
            var raw = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"[ReviewService] GetUserRatingAsync - BookId: {bookId}, Status: {response.StatusCode}, Raw: {raw?.Substring(0, Math.Min(200, raw?.Length ?? 0))}");
            
            // Handle 204 No Content (shouldn't happen with current backend, but handle it)
            if (response.StatusCode == HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(raw))
            {
                var noContentData = new UserRatingDto { BookId = bookId, Rating = 0 };
                return ReviewServiceResult<UserRatingDto>.SuccessResult(noContentData, "No rating found.");
            }
            
            var serverMessage = DeserializeServerMessage<UserRatingDto>(raw);
            UserRatingDto? ratingData = null;
            string message = "Rating retrieved successfully.";

            if (serverMessage != null && serverMessage.Data != null)
            {
                // Response is wrapped in { message, data } format
                ratingData = serverMessage.Data;
                message = serverMessage.GetMessageOr("Rating retrieved successfully.");
            }
            else if (!string.IsNullOrWhiteSpace(raw))
            {
                // Try to deserialize directly as UserRatingDto (in case backend returns data directly)
                try
                {
                    ratingData = JsonSerializer.Deserialize<UserRatingDto>(raw, _jsonOptions);
                    Console.WriteLine($"[ReviewService] GetUserRatingAsync - Direct deserialization successful for book {bookId}");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[ReviewService] GetUserRatingAsync - Direct deserialization failed: {ex.Message}");
                }
            }

            if (response.IsSuccessStatusCode)
            {
                var finalData = ratingData ?? new UserRatingDto { BookId = bookId, Rating = 0 };
                Console.WriteLine($"[ReviewService] GetUserRatingAsync - Final rating: {finalData.Rating} for book {bookId}");
                return ReviewServiceResult<UserRatingDto>.SuccessResult(finalData, message);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // For GetUserRatingAsync, return success with rating 0 instead of unauthorized
                // This allows the UI to load gracefully without errors when token is missing/expired
                // The Discover page will handle authentication state separately
                var unauthorizedData = new UserRatingDto { BookId = bookId, Rating = 0 };
                return ReviewServiceResult<UserRatingDto>.SuccessResult(unauthorizedData, "No rating found.");
            }

            var errorMessage = serverMessage?.GetMessageOr("Unable to retrieve rating.") ?? "Unable to retrieve rating.";
            return ReviewServiceResult<UserRatingDto>.Failure(errorMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ReviewService] GetUserRatingAsync exception for book {bookId}: {ex.Message}");
            // On exception, return success with rating 0 to allow graceful degradation
            var exceptionData = new UserRatingDto { BookId = bookId, Rating = 0 };
            return ReviewServiceResult<UserRatingDto>.SuccessResult(exceptionData, "Unable to retrieve rating.");
        }
    }

    public async Task<ReviewServiceResult<UserRatingDto>> RateBookAsync(int bookId, int rating)
    {
        try
        {
            var dto = new RateBookDto { BookId = bookId, Rating = rating };
            
            // Debug: Check if Authorization header is present
            Console.WriteLine($"[ReviewService] RateBookAsync - BaseAddress: {_http.BaseAddress}");
            Console.WriteLine($"[ReviewService] RateBookAsync - Has DefaultRequestHeaders.Authorization: {_http.DefaultRequestHeaders.Authorization != null}");
            
            var response = await _http.PostAsJsonAsync("api/Review/rate", dto);
            var raw = await response.Content.ReadAsStringAsync();
            var serverMessage = DeserializeServerMessage<UserRatingDto>(raw);
            
            Console.WriteLine($"[ReviewService] RateBookAsync - Response Status: {response.StatusCode}");
            Console.WriteLine($"[ReviewService] RateBookAsync - Response Content: {raw.Substring(0, Math.Min(200, raw.Length))}");

            if (response.IsSuccessStatusCode)
            {
                var data = serverMessage?.Data ?? new UserRatingDto { BookId = bookId, Rating = rating };
                var message = serverMessage?.GetMessageOr("Rating saved successfully.") ?? "Rating saved successfully.";
                return ReviewServiceResult<UserRatingDto>.SuccessResult(data, message);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var message = serverMessage?.GetMessageOr("Login to rate this book.") ?? "Login to rate this book.";
                return ReviewServiceResult<UserRatingDto>.Unauthorized(message);
            }

            var errorMessage = serverMessage?.GetMessageOr("Unable to rate this book.") ?? "Unable to rate this book.";
            return ReviewServiceResult<UserRatingDto>.Failure(errorMessage);
        }
        catch (Exception ex)
        {
            return ReviewServiceResult<UserRatingDto>.Failure($"Unable to rate this book: {ex.Message}");
        }
    }

    private ServerMessage<T>? DeserializeServerMessage<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Console.WriteLine($"[ReviewService] DeserializeServerMessage - Empty JSON");
            return null;
        }

        try
        {
            var result = JsonSerializer.Deserialize<ServerMessage<T>>(json, _jsonOptions);
            var hasData = result != null && result.Data != null;
            Console.WriteLine($"[ReviewService] DeserializeServerMessage - Success: {result != null}, HasData: {hasData}");
            return result;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[ReviewService] DeserializeServerMessage - JSON Exception: {ex.Message}, JSON: {json.Substring(0, Math.Min(200, json.Length))}");
            return null;
        }
    }
}

public record ReviewServiceResult<T>(bool Success, bool IsUnauthorized, string Message, T? Data)
{
    public static ReviewServiceResult<T> SuccessResult(T? data, string message) =>
        new(true, false, message, data);

    public static ReviewServiceResult<T> Failure(string message) =>
        new(false, false, message, default);

    public static ReviewServiceResult<T> Unauthorized(string message) =>
        new(false, true, message, default);
}
