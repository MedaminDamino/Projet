using API.DTO;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingGoalController : ControllerBase
    {
        private readonly IReadingGoalRepo _repo;
        private readonly IBookRepo _bookRepo;

        public ReadingGoalController(IReadingGoalRepo repo, IBookRepo bookRepo)
        {
            _repo = repo;
            _bookRepo = bookRepo;
        }

        private string? GetUserId() =>
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.FindFirstValue("sub")
            ?? User?.FindFirstValue(ClaimTypes.Name);

        private IActionResult UnauthorizedApiResponse() => Unauthorized(new ApiResponse<object>
        {
            Success = false,
            Message = "User not authenticated",
            ErrorCode = "UNAUTHORIZED"
        });

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return UnauthorizedApiResponse();
            }

            var goals = (await _repo.GetAllAsync())
                .Where(g => g.ApplicationUserId == userId)
                .ToList();

            return Ok(new ApiResponse<IEnumerable<ReadingGoal>>
            {
                Success = true,
                Message = "Goals loaded",
                Data = goals
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return UnauthorizedApiResponse();
            }

            var goal = await _repo.GetByIdAsync(id);
            if (goal == null || goal.ApplicationUserId != userId) return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Goal not found",
                ErrorCode = "NOT_FOUND"
            });
            return Ok(new ApiResponse<ReadingGoal>
            {
                Success = true,
                Message = "Goal loaded",
                Data = goal
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReadingGoalCreateDto goalDto)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return UnauthorizedApiResponse();
            }

            if (goalDto.BookId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please select a book for this goal.",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            // Ensure the referenced book exists
            var book = await _bookRepo.GetByIdAsync(goalDto.BookId);
            if (book == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Book not found.",
                    ErrorCode = "BOOK_NOT_FOUND"
                });
            }

            if (await _repo.ExistsForUserYearBookAsync(userId, goalDto.Year, goalDto.BookId))
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "You already have a goal for this book in this year.",
                    ErrorCode = "DUPLICATE_BOOK_GOAL"
                });
            }

            var goal = new ReadingGoal
            {
                Year = goalDto.Year,
                Goal = goalDto.Goal,
                Progress = goalDto.Progress,
                BookId = goalDto.BookId,
                CreatedAt = DateTime.UtcNow,
                ApplicationUserId = userId,
                Book = book
            };

            await _repo.AddAsync(goal);
            return Ok(new ApiResponse<ReadingGoal>
            {
                Success = true,
                Message = "Goal created.",
                Data = goal
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReadingGoalUpdateDto updated)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return UnauthorizedApiResponse();
            }

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Goal not found",
                    ErrorCode = "NOT_FOUND"
                });
            }

            if (existing.ApplicationUserId != null && existing.ApplicationUserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<object>
                {
                    Success = false,
                    Message = "You cannot modify this goal.",
                    ErrorCode = "FORBIDDEN"
                });
            }

            if (updated.BookId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please select a book for this goal.",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var book = await _bookRepo.GetByIdAsync(updated.BookId);
            if (book == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Book not found.",
                    ErrorCode = "BOOK_NOT_FOUND"
                });
            }

            if (await _repo.ExistsForUserYearBookAsync(userId, updated.Year, updated.BookId, excludeId: id))
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "You already have a goal for this book in this year.",
                    ErrorCode = "DUPLICATE_BOOK_GOAL"
                });
            }

            existing.Year = updated.Year;
            existing.Goal = updated.Goal;
            existing.Progress = updated.Progress;
            existing.BookId = updated.BookId;
            existing.ApplicationUserId = userId;
            existing.Book = book;

            await _repo.UpdateAsync(existing);

            return Ok(new ApiResponse<ReadingGoal>
            {
                Success = true,
                Message = "Goal updated.",
                Data = existing
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return UnauthorizedApiResponse();
            }

            var goal = await _repo.GetByIdAsync(id);
            if (goal == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Goal not found",
                    ErrorCode = "NOT_FOUND"
                });
            }

            if (goal.ApplicationUserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse<object>
                {
                    Success = false,
                    Message = "You cannot delete this goal.",
                    ErrorCode = "FORBIDDEN"
                });
            }

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Goal not found",
                ErrorCode = "NOT_FOUND"
            });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Goal deleted."
            });
        }
    }
}
