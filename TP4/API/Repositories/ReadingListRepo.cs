using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class ReadingListRepo : IReadingListRepo
    {
        private readonly ApplicationContext _context;

        public ReadingListRepo(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadingList>> GetAllAsync()
        {
            return await _context.ReadingLists
                .Include(r => r.Book)
                .ToListAsync();
        }

        public async Task<ReadingList?> GetByIdAsync(int id)
        {
            return await _context.ReadingLists
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.ReadingListID == id);
        }

        public async Task AddAsync(ReadingList list)
        {
            _context.ReadingLists.Add(list);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ReadingList list)
        {
            _context.ReadingLists.Update(list);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var list = await _context.ReadingLists.FindAsync(id);
            if (list == null) return false;

            _context.ReadingLists.Remove(list);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
