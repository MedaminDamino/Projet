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

        private string? GetUserId() =>
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.FindFirstValue("sub")
            ?? User?.FindFirstValue(ClaimTypes.Name);

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
                return HandleUnexpectedError(ex, "loading goals");
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
                return HandleUnexpectedError(ex, "loading the requested goal");
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Create(ReadingGoalCreateDto goalDto)
        {
            try
            {
                var userId = GetUserId();

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
                return HandleUnexpectedError(ex, "creating the goal");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Update(int id, ReadingGoalUpdateDto updated)
        {
            try
            {
                var userId = GetUserId();

                var existing = await _repo.GetByIdAsync(id);
                if (existing == null)
                {
                    return NotFound(new { message = "Goal not found." });
                }

                if (updated.BookId <= 0)
                {
                    return BadRequest(new { message = "Please select a book for this goal." });
                }

                var validationError = ValidateGoalInputs(updated.Year, updated.GoalPercentage, updated.Progress);
                if (validationError != null)
                {
                    return BadRequest(new { message = validationError });
                }

                var book = await _bookRepo.GetByIdAsync(updated.BookId);
                if (book == null)
                {
                    return BadRequest(new { message = "Book not found." });
                }

                if (await _repo.ExistsForUserYearBookAsync(userId, updated.Year, updated.BookId, excludeId: id))
                {
                    return Conflict(new { message = "You already have a goal for this book in this year." });
                }

                existing.Year = updated.Year;
                existing.GoalPercentage = updated.GoalPercentage;
                existing.Progress = updated.Progress;
                existing.BookId = updated.BookId;
                existing.ApplicationUserId = userId;
                existing.Book = book;

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
                return HandleUnexpectedError(ex, "updating the goal");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Note: Admin role is excluded - only SuperAdmin and User (for their own goals) can delete

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
                return HandleUnexpectedError(ex, "deleting the goal");
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

            if (progress < 1 || progress > 100)
            {
                return "Progress must be between 1% and 100%.";
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

        private IActionResult HandleUnexpectedError(Exception ex, string action)
        {
            _ = ex;
            return StatusCode(500, new
            {
                message = $"An unexpected error occurred while {action}. Please try again later."
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
