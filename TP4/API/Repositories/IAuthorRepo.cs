using API.Models;

namespace API.Interfaces
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAsync();
        Task<Author?> GetByIdAsync(int id);
        Task<Author> AddAsync(Author author);
        Task<bool> UpdateAsync(Author author);
        Task<bool> DeleteAsync(int id);
    }
}
