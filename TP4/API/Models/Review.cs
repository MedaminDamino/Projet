using System.Text.Json.Serialization;

namespace API.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
