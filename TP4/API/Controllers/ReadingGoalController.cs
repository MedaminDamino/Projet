using API.DTO;
using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

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

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // ============================================================================
        // ENDPOINTS FOR DISCOVER PAGE (Quick Goal Creation)
        // ============================================================================

        /// <summary>
        /// Create a reading goal from the Discover page for a specific book
        /// </summary>
        [HttpPost("from-book")]
        [Authorize]
        public async Task<IActionResult> CreateQuickGoal(ReadingGoalCreateDto goalDto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            try
            {
                if (goalDto.BookId <= 0)
                {
                    return BadRequest(new { message = "Please select a book for this goal." });
                }

                var validationError = ValidateGoalInputs(goalDto.Year, goalDto.GoalPercentage, goalDto.Progress);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                var book = await _bookRepo.GetByIdAsync(goalDto.BookId);
                if (book == null)
                {
                    return BadRequest(new { message = "Book not found." });
                }

                // Check if user already has a goal for this book in this year
                if (await _repo.ExistsForUserYearBookAsync(userId, goalDto.Year, goalDto.BookId))
                {
                    return Conflict(new { message = "You already have a goal for this book in this year." });
                }

                var goal = new ReadingGoal
                {
                    Year = goalDto.Year,
                    GoalPercentage = goalDto.GoalPercentage,
                    Progress = goalDto.Progress,
                    BookId = goalDto.BookId,
                    CreatedAt = DateTime.UtcNow,
                    ApplicationUserId = userId,
                    Book = book
                };

                await _repo.AddAsync(goal);
                Console.WriteLine($"[ReadingGoalController] Created goal for user {userId}, book {goalDto.BookId}, year {goalDto.Year}");

                return Ok(new
                {
                    message = $"Goal created for '{book.Title}'.",
                    data = BuildGoalResponse(goal)
                });
            }
            catch (DbUpdateException dbEx)
            {
                return HandleDatabaseError(dbEx, "creating the goal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error creating quick goal: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the goal." });
            }
        }

        /// <summary>
        /// Get all reading goals for the current authenticated user
        /// </summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserGoals()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            try
            {
                var goals = await _repo.GetAllAsync();
                var userGoals = goals.Where(g => g.ApplicationUserId == userId).ToList();

                return Ok(new
                {
                    success = true,
                    message = "User goals retrieved successfully",
                    data = userGoals.Select(BuildGoalResponse)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error loading user goals: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading your goals." });
            }
        }

        /// <summary>
        /// Get the user's reading goal for a specific book
        /// </summary>
        [HttpGet("user/{bookId}")]
        [Authorize]
        public async Task<IActionResult> GetGoalForBook(int bookId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            try
            {
                var goals = await _repo.GetAllAsync();
                var goal = goals.FirstOrDefault(g => g.ApplicationUserId == userId && g.BookId == bookId);

                if (goal == null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "No goal found for this book",
                        data = (object?)null
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Goal retrieved successfully",
                    data = BuildGoalResponse(goal)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error loading goal for book {bookId}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading the goal." });
            }
        }

        // ============================================================================
        // LEGACY ENDPOINTS (Admin/Management)
        // ============================================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var goals = await _repo.GetAllAsync();

                return Ok(new
                {
                    message = "Goals loaded successfully.",
                    data = goals.Select(BuildGoalResponse)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error loading all goals: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading goals." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var goal = await _repo.GetByIdAsync(id);
                if (goal == null)
                {
                    return NotFound(new { message = "Goal not found." });
                }

                return Ok(new
                {
                    message = "Goal loaded successfully.",
                    data = BuildGoalResponse(goal)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error loading goal {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while loading the goal." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Create(ReadingGoalCreateDto goalDto)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (goalDto.BookId <= 0)
                {
                    return BadRequest(new { message = "Please select a book for this goal." });
                }

                var validationError = ValidateGoalInputs(goalDto.Year, goalDto.GoalPercentage, goalDto.Progress);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                var book = await _bookRepo.GetByIdAsync(goalDto.BookId);
                if (book == null)
                {
                    return BadRequest(new { message = "Book not found." });
                }

                if (await _repo.ExistsForUserYearBookAsync(userId, goalDto.Year, goalDto.BookId))
                {
                    return Conflict(new { message = "You already have a goal for this book in this year." });
                }

                var goal = new ReadingGoal
                {
                    Year = goalDto.Year,
                    GoalPercentage = goalDto.GoalPercentage,
                    Progress = goalDto.Progress,
                    BookId = goalDto.BookId,
                    CreatedAt = DateTime.UtcNow,
                    ApplicationUserId = userId,
                    Book = book
                };

                await _repo.AddAsync(goal);
                return Ok(new
                {
                    message = "Goal created successfully.",
                    data = BuildGoalResponse(goal)
                });
            }
            catch (DbUpdateException dbEx)
            {
                return HandleDatabaseError(dbEx, "creating the goal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error creating goal: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the goal." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Update(int id, ReadingGoalUpdateDto updated)
        {
            try
            {
                var userId = GetCurrentUserId();

                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Goal not found." });
                }

                var validationError = ValidateGoalInputs(updated.Year, updated.GoalPercentage, updated.Progress);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                // Note: BookId doesn't change in update, so we keep the existing book
                if (await _repo.ExistsForUserYearBookAsync(userId, updated.Year, existing.BookId, excludeId: id))
                {
                    return Conflict(new { message = "You already have a goal for this book in this year." });
                }

                existing.Year = updated.Year;
                existing.GoalPercentage = updated.GoalPercentage;
                existing.Progress = updated.Progress;
                existing.ApplicationUserId = userId;

                await _repo.UpdateAsync(existing);

                return Ok(new
                {
                    message = "Goal updated successfully.",
                    data = BuildGoalResponse(existing)
                });
            }
            catch (DbUpdateException dbEx)
            {
                return HandleDatabaseError(dbEx, "updating the goal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error updating goal: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred while updating the goal." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var goal = await _repo.GetByIdAsync(id);
                if (goal == null)
                {
                    return NotFound(new { message = "Goal not found." });
                }

                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, new { message = "Unable to delete goal. Please try again." });
                }

                return Ok(new { message = "Goal deleted successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                return HandleDatabaseError(dbEx, "deleting the goal");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadingGoalController] Error deleting goal: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the goal." });
            }
        }

        private static string? ValidateGoalInputs(int year, int goalPercentage, int progress)
        {
            var currentYear = DateTime.UtcNow.Year;
            if (year <= currentYear)
            {
                return "Goal year must be in the future.";
            }

            if (goalPercentage < 1 || goalPercentage > 100)
            {
                return "Goal percentage must be between 1% and 100%.";
            }

            if (progress < 0 || progress > 100)
            {
                return "Progress must be between 0% and 100%.";
            }

             if (progress > goalPercentage)
            {
                return "Progress cannot exceed the goal percentage.";
            }

            return null;
        }

        private IActionResult HandleDatabaseError(DbUpdateException ex, string action)
        {
            if (IsDuplicateGoalError(ex))
            {
                return Conflict(new { message = "You already have a goal for this book in this year." });
            }

            return StatusCode(500, new
            {
                message = $"We ran into a database issue while {action}. Please try again."
            });
        }

        private static bool IsDuplicateGoalError(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            return message.Contains("IX_ReadingGoals_ApplicationUserId_Year_BookId", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);
        }

        private static object BuildGoalResponse(ReadingGoal goal)
        {
            return new
            {
                goal.Id,
                goal.UserId,
                goal.ApplicationUserId,
                goal.Year,
                goal.GoalPercentage,
                goal.Progress,
                goal.BookId,
                goal.CreatedAt,
                Book = goal.Book == null
                    ? null
                    : new
                    {
                        goal.Book.BookId,
                        goal.Book.Title,
                        goal.Book.AuthorId,
                        goal.Book.GenreId,
                        goal.Book.PublishYear,
                        goal.Book.Description,
                        goal.Book.ImageUrl
                    }
            };
        }
    }
}
