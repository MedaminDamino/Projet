using API.Helpers;
using API.Interfaces;
using API.Models;
using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

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
            return Ok(new ServerMessage<IEnumerable<Comment>>
            {
                Message = "All comments retrieved successfully.",
                Data = comments
            });
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetByUser()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var comments = await _repo.GetByUserAsync(userId);
            
            // If no comments, return empty list (200 OK), not error
            if (comments == null || !comments.Any())
            {
                return Ok(new ServerMessage<IEnumerable<CommentReadDto>>
                {
                    Message = "No comments found for this user.",
                    Data = new List<CommentReadDto>()
                });
            }

            var dtoList = comments.Select(c => new CommentReadDto
            {
                CommentID = c.CommentID,
                ReviewID = c.ReviewID,
                BookId = c.Review?.BookId ?? 0,
                BookTitle = c.Review?.Book?.Title ?? "Unknown book",
                CommentText = c.CommentText,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(new ServerMessage<IEnumerable<CommentReadDto>>
            {
                Message = "User comments retrieved successfully.",
                Data = dtoList
            });
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

        private string? GetCurrentUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue("sub")
                ?? User?.FindFirstValue(ClaimTypes.Name);
        }
    }
}
