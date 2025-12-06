using Microsoft.AspNetCore.Identity;

namespace API.Models {
    public class ApplicationUser : IdentityUser {
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ReadingList> ReadingLists { get; set; } = new List<ReadingList>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<ReadingGoal> ReadingGoals { get; set; } = new List<ReadingGoal>();
    } 
}
