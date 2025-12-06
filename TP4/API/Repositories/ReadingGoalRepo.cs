using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class ReadingGoalRepo : IReadingGoalRepo
    {
        private readonly ApplicationContext _context;

        public ReadingGoalRepo(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadingGoal>> GetAllAsync() =>
            await _context.ReadingGoals
                .Include(g => g.Book)
                .ToListAsync();

        public async Task<ReadingGoal?> GetByIdAsync(int id)
        {
            return await _context.ReadingGoals
                .Include(g => g.Book)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(ReadingGoal goal)
        {
            _context.ReadingGoals.Add(goal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReadingGoal goal)
        {
            _context.ReadingGoals.Update(goal);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var goal = await _context.ReadingGoals.FindAsync(id);
            if (goal == null) return false;

            _context.ReadingGoals.Remove(goal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsForUserYearBookAsync(string? applicationUserId, int year, int bookId, int? excludeId = null)
        {
            return await _context.ReadingGoals.AnyAsync(g =>
                g.ApplicationUserId == applicationUserId &&
                g.Year == year &&
                g.BookId == bookId &&
                (!excludeId.HasValue || g.Id != excludeId.Value));
        }
    }
}
