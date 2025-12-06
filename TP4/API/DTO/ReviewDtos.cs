using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.DTO
{
    public class UserRatingDto
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
    }

    public class RateBookDto
    {
        [Required]
        public int BookId { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
    }

    public class ReviewReadDto
    {
        [JsonPropertyName("reviewID")]
        public int ReviewID { get; set; }

        [JsonPropertyName("appUserID")]
        public string AppUserID { get; set; } = string.Empty;

        [JsonPropertyName("bookId")]
        public int BookId { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("reviewText")]
        public string? ReviewText { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
