using API.Models;
using System.Text.Json.Serialization;
public class Comment
{
    public int CommentID { get; set; }
    public int ReviewID { get; set; }
    public int AppUserID { get; set; }
    public string CommentText { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public Review? Review { get; set; } = null!;
}
