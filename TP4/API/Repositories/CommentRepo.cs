using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace API.Repositories
{
    public class CommentRepo : ICommentRepo
    {
        private readonly ApplicationContext _context;

        public CommentRepo(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments
                .Include(c => c.Review)
                .ThenInclude(r => r!.Book)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Review)
                .ThenInclude(r => r!.Book)
                .FirstOrDefaultAsync(c => c.CommentID == id);
        }

        public async Task AddAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Comment>> GetByUserAsync(string userId)
        {
            return await _context.Comments
                .Include(c => c.Review)
                .ThenInclude(r => r!.Book)
                .Where(c => c.ApplicationUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
