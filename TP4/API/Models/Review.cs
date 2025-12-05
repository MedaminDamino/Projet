using API.Models;
using System.Text.Json.Serialization;

public class Review
{
    public int ReviewID { get; set; }
    public int AppUserID { get; set; }
    public int BookId { get; set; }
    public int Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
    [JsonIgnore]

    public Book? Book { get; set; } = null!;
    [JsonIgnore]

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

}
