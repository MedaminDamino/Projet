    using API.Interfaces;
    using API.Models;
    using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class GenreRepository : IGenreRepository
        {
            private readonly ApplicationContext _context;

            public GenreRepository(ApplicationContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Genre>> GetAllAsync()
            {
                return await _context.Genres.ToListAsync();
            }

            public async Task<Genre?> GetByIdAsync(int id)
            {
                return await _context.Genres.FirstOrDefaultAsync(g => g.GenreId == id);
            }

            public async Task<Genre> AddAsync(Genre genre)
            {
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();
                return genre;
            }

            public async Task<bool> UpdateAsync(Genre genre)
            {
                _context.Genres.Update(genre);
                return await _context.SaveChangesAsync() > 0;
            }

            public async Task<bool> DeleteAsync(int id)
            {
                var genre = await _context.Genres.FindAsync(id);
                if (genre == null)
                    return false;

                _context.Genres.Remove(genre);
                return await _context.SaveChangesAsync() > 0;
            }
        }
    }
