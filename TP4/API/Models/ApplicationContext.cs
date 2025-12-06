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

    modelBuilder.Entity<Book>()
        .HasOne(b => b.Author)
        .WithMany(a => a.Books!)
        .HasForeignKey(b => b.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Genre>()
        .HasMany(g => g.Books)
        .WithOne(b => b.Genre)
        .HasForeignKey(b => b.GenreId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Book>()
        .HasMany(b => b.Reviews)
        .WithOne(r => r.Book)
        .HasForeignKey(r => r.BookId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<ReadingList>()
        .HasOne(r => r.Book)
        .WithMany(b => b.ReadingLists)
        .HasForeignKey(r => r.BookId);

    modelBuilder.Entity<ReadingList>()
        .HasOne(r => r.ApplicationUser)
        .WithMany(u => u.ReadingLists)
        .HasForeignKey(r => r.ApplicationUserId)
        .IsRequired(false);

    modelBuilder.Entity<ReadingGoal>()
        .HasOne(g => g.Book)
        .WithMany()
        .HasForeignKey(g => g.BookId)
        .OnDelete(DeleteBehavior.Cascade); // or Restrict if you want to block

    modelBuilder.Entity<ReadingGoal>()
        .HasOne(g => g.ApplicationUser)
        .WithMany(u => u.ReadingGoals)
        .HasForeignKey(g => g.ApplicationUserId)
        .IsRequired(false);

    modelBuilder.Entity<ReadingGoal>()
        .HasIndex(g => new { g.ApplicationUserId, g.Year, g.BookId })
        .IsUnique();
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
