using API.Models;

namespace API.Interfaces
{
    public interface ICommentRepo
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(int id);
    }
}
