using API.Models;

public class ReadingList
{
    public int ReadingListID { get; set; }

    // Use BookId (with lowercase 'd') to match EF Core conventions
    public int BookId { get; set; }

    public string Status { get; set; } = "Not Started";
    public DateTime AddedAt { get; set; }

    // Use string to match ApplicationUser.Id type
    public string? ApplicationUserId { get; set; }

    // Navigation properties
    public Book? Book { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}