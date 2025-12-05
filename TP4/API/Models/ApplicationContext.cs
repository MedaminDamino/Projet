using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Models
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationContext(DbContextOptions options) :base(options)  {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReadingList>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId);  // Changed from BookID to BookId

            modelBuilder.Entity<ReadingList>()
                .HasOne(r => r.ApplicationUser)
                .WithMany()
                .HasForeignKey(r => r.ApplicationUserId)
                .IsRequired(false);
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<ReadingList> ReadingLists { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ReadingGoal> ReadingGoals { get; set; }

    }
}