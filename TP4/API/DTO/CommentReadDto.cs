using System.Text.Json.Serialization;

namespace API.DTO
{
    public class CommentReadDto
    {
        [JsonPropertyName("commentID")]
        public int CommentID { get; set; }

        [JsonPropertyName("reviewID")]
        public int ReviewID { get; set; }

        [JsonPropertyName("bookId")]
        public int BookId { get; set; }

        [JsonPropertyName("bookTitle")]
        public string BookTitle { get; set; } = string.Empty;

        [JsonPropertyName("commentText")]
        public string CommentText { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
