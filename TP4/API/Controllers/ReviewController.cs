using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepo _repo;

        public ReviewController(IReviewRepo repo)
        {
            _repo = repo;
        }

        // GET api/review
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _repo.GetAllAsync();
            return Ok(reviews);
        }

        // GET api/review/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await _repo.GetByIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        // POST api/review
        [HttpPost]
        public async Task<IActionResult> Create(Review review)
        {
            review.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(review);

            return CreatedAtAction(nameof(GetById), new { id = review.ReviewID }, review);
        }

        // PUT api/review/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Review updated)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Rating = updated.Rating;
            existing.ReviewText = updated.ReviewText;

            await _repo.UpdateAsync(existing);

            return NoContent();
        }

        // DELETE api/review/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
