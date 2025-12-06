using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BookDashboardBlazor.Models;

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterModel
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int GenreId { get; set; }
    public int? PublishYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class BookDiscover
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public int GenreId { get; set; }
    public int? PublishYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class Genre
{
    [JsonPropertyName("genreId")]
    public int GenreId { get; set; }
    [JsonPropertyName("genreName")]
    public string GenreName { get; set; } = string.Empty;
}

public class Review
{
    [JsonPropertyName("reviewID")]
    public int ReviewID { get; set; }
    [JsonPropertyName("appUserID")]
    public int AppUserID { get; set; }
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }
    [JsonPropertyName("rating")]
    public int Rating { get; set; }
    [JsonPropertyName("reviewText")]
    public string ReviewText { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class Comment
{
    [JsonPropertyName("commentID")]
    public int CommentID { get; set; }
    [JsonPropertyName("reviewID")]
    public int ReviewID { get; set; }
    [JsonPropertyName("appUserID")]
    public int AppUserID { get; set; }
    [JsonPropertyName("commentText")]
    public string CommentText { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class ReadingGoal
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    [JsonPropertyName("applicationUserId")]
    public string? ApplicationUserId { get; set; }
    [JsonPropertyName("year")]
    public int Year { get; set; }
    [JsonPropertyName("goal")]
    public int Goal { get; set; }
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }
    [JsonPropertyName("book")]
    public Book? Book { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class ReadingList
{
    [JsonPropertyName("readingListID")]
    public int ReadingListID { get; set; }
    
    [JsonPropertyName("applicationUserId")]
    public string? ApplicationUserId { get; set; }
    
    // Keep for backward compatibility
    [JsonPropertyName("appUserID")]
    public int AppUserID { get; set; }
    
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "Not Started";
    
    [JsonPropertyName("addedAt")]
    public DateTime AddedAt { get; set; }
    
    [JsonPropertyName("book")]
    public Book? Book { get; set; }
}

// DTO for API responses - handles nullable fields from API
public class ReadingListDto
{
    [JsonPropertyName("readingListID")]
    public int ReadingListID { get; set; }
    
    [JsonPropertyName("appUserID")]
    public int AppUserID { get; set; }
    
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "Not Started";
    
    [JsonPropertyName("addedAt")]
    public DateTime AddedAt { get; set; }
    
    [JsonPropertyName("book")]
    public BookDto? Book { get; set; }
}

public class BookDto
{
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("authorId")]
    public int? AuthorId { get; set; }
    
    [JsonPropertyName("genreId")]
    public int? GenreId { get; set; }
    
    [JsonPropertyName("publishYear")]
    public int? PublishYear { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}

// API Response wrapper
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    public string GetUserMessage(string fallback = "Operation failed.")
    {
        if (!string.IsNullOrWhiteSpace(Message))
        {
            return Message;
        }

        return !string.IsNullOrWhiteSpace(ErrorCode)
            ? $"{fallback} (code: {ErrorCode})"
            : fallback;
    }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

public class RoleDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("normalizedName")]
    public string NormalizedName { get; set; } = string.Empty;
}

public class RoleCreateDto
{
    [Required]
    [MinLength(2)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class RoleUpdateDto
{
    [Required]
    [MinLength(2)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class SetUserRoleDto
{
    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("roleName")]
    public string RoleName { get; set; } = string.Empty;
}

public class UserWithRolesDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}

public class AssignRoleDto
{
    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("roleIdOrName")]
    public string RoleIdOrName { get; set; } = string.Empty;
}

public class RemoveRoleDto
{
    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("roleIdOrName")]
    public string RoleIdOrName { get; set; } = string.Empty;
}
