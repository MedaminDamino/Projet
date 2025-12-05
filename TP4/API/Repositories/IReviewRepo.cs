using API.Models;

namespace API.Interfaces
{
    public interface IReviewRepo
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(int id);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task<bool> DeleteAsync(int id);
    }
}
