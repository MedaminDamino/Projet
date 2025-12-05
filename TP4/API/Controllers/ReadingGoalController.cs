using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingGoalController : ControllerBase
    {
        private readonly IReadingGoalRepo _repo;

        public ReadingGoalController(IReadingGoalRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var goal = await _repo.GetByIdAsync(id);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReadingGoal goal)
        {
            goal.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(goal);
            return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReadingGoal updated)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Year = updated.Year;
            existing.Goal = updated.Goal;
            existing.Progress = updated.Progress;
            existing.UserId = updated.UserId;

            await _repo.UpdateAsync(existing);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
