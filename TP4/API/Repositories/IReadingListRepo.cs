using API.Models;

namespace API.Interfaces
{
    public interface IReadingListRepo
    {
        Task<IEnumerable<ReadingList>> GetAllAsync();
        Task<ReadingList?> GetByIdAsync(int id);
        Task AddAsync(ReadingList list);
        Task UpdateAsync(ReadingList list);
        Task<bool> DeleteAsync(int id);
    }
}
