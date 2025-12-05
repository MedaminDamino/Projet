using API.Models;

namespace API.Interfaces
{
    public interface IReadingGoalRepo
    {
        Task<IEnumerable<ReadingGoal>> GetAllAsync();
        Task<ReadingGoal?> GetByIdAsync(int id);
        Task AddAsync(ReadingGoal goal);
        Task UpdateAsync(ReadingGoal goal);
        Task<bool> DeleteAsync(int id);
    }
}
