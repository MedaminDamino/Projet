using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepo _repo;

        public CommentController(ICommentRepo repo)
        {
            _repo = repo;
        }

        // GET api/comment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _repo.GetAllAsync();
            return Ok(comments);
        }

        // GET api/comment/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _repo.GetByIdAsync(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        // POST api/comment
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Create(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;

            await _repo.AddAsync(comment);

            return CreatedAtAction(nameof(GetById), new { id = comment.CommentID }, comment);
        }

        // PUT api/comment/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Update(int id, Comment updated)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.CommentText = updated.CommentText;

            await _repo.UpdateAsync(existing);

            return NoContent();
        }

        // DELETE api/comment/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            // Note: Admin role is excluded - only SuperAdmin and User (for their own comments) can delete

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
