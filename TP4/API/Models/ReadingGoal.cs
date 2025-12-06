namespace API.Models
{
    public class ReadingGoal
    {
        public int Id { get; set; }

        // Legacy numeric user id (kept for compatibility)
        public int UserId { get; set; }

        // Identity user id
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int Year { get; set; }
        public int Goal { get; set; }
        public int Progress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
