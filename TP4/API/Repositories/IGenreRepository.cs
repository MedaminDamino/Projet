using API.Models;

namespace API.Interfaces
{
    public interface IGenreRepository
    {
        Task<IEnumerable<Genre>> GetAllAsync();
        Task<Genre?> GetByIdAsync(int id);
        Task<Genre> AddAsync(Genre genre);
        Task<bool> UpdateAsync(Genre genre);
        Task<bool> DeleteAsync(int id);
        Task<bool> HasBooksAsync(int genreId);
    }
}
