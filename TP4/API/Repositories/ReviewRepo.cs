using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace API.Repositories
{
    public class ReviewRepo : IReviewRepo
    {
        private readonly ApplicationContext _context;

        public ReviewRepo(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Comments)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ReviewId == id);
        }

        public async Task AddAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Review?> GetByUserAndBookAsync(string userId, int bookId)
        {
            return await _context.Reviews
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.ApplicationUserId == userId && r.BookId == bookId);
        }

        public async Task<bool> ExistsForBookAsync(int bookId)
        {
            return await _context.Reviews.AnyAsync(r => r.BookId == bookId);
        }

        public async Task<IEnumerable<Review>> GetByUserAsync(string userId)
        {
            return await _context.Reviews
                .Include(r => r.Book)
                .Where(r => r.ApplicationUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
