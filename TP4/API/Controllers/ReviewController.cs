using System.Linq;
using System.Security.Claims;
using API.DTO;
using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var reviews = await _repo.GetAllAsync();
                var dtoList = reviews.Select(r => new ReviewReadDto
                {
                    ReviewID = r.ReviewId,
                    AppUserID = r.ApplicationUserId,
                    BookId = r.BookId,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return Ok(new { message = "Reviews loaded successfully.", data = dtoList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while loading reviews.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var review = await _repo.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            return Ok(review);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetByUser()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "You must be logged in to view your reviews." });
            }

            var reviews = await _repo.GetByUserAsync(userId);
            var dtoList = reviews.Select(r => new ReviewReadDto
            {
                ReviewID = r.ReviewId,
                AppUserID = r.ApplicationUserId,
                BookId = r.BookId,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(new
            {
                message = "User reviews retrieved successfully.",
                data = dtoList
            });
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = BuildModelStateMessage("Invalid review payload.") });
            }

            if (dto.BookId <= 0)
            {
                return BadRequest(new { message = "A valid bookId is required." });
            }

            if (IsRatingInvalid(dto.Rating))
            {
                return BadRequest(new { message = BuildInvalidRatingMessage(dto.Rating) });
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "You must be logged in to create a review." });
            }

            var review = new Review
            {
                ApplicationUserId = userId,
                BookId = dto.BookId,
                Rating = dto.Rating,
                ReviewText = NormalizeReviewText(dto.ReviewText),
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(review);

            return CreatedAtAction(nameof(GetById), new { id = review.ReviewId }, new
            {
                message = "Review created successfully.",
                data = BuildReviewResponse(review)
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Update(int id, [FromBody] ReviewUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = BuildModelStateMessage("Invalid review payload.") });
            }

            if (dto.BookId <= 0)
            {
                return BadRequest(new { message = "A valid bookId is required." });
            }

            if (IsRatingInvalid(dto.Rating))
            {
                return BadRequest(new { message = BuildInvalidRatingMessage(dto.Rating) });
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "You must be logged in to update a review." });
            }

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            existing.BookId = dto.BookId;
            existing.Rating = dto.Rating;
            existing.ReviewText = NormalizeReviewText(dto.ReviewText);
            existing.ApplicationUserId = existing.ApplicationUserId ?? userId;

            await _repo.UpdateAsync(existing);

            return Ok(new
            {
                message = "Review updated successfully.",
                data = BuildReviewResponse(existing)
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            // Note: Admin role is excluded - only SuperAdmin and User (for their own reviews) can delete

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Review not found." });
            }

            return Ok(new { message = "Review deleted successfully." });
        }

        [HttpGet("user-rating")]
        [Authorize]
        public async Task<IActionResult> GetUserRating([FromQuery] int bookId)
        {
            if (bookId <= 0)
            {
                return BadRequest(new { message = "A valid bookId is required." });
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "You must be logged in to view your rating." });
            }

            var review = await _repo.GetByUserAndBookAsync(userId, bookId);
            var ratingValue = review?.Rating ?? 0;

            return Ok(new
            {
                message = ratingValue > 0
                    ? "Rating retrieved successfully."
                    : "No rating found for this book.",
                data = new UserRatingDto
                {
                    BookId = bookId,
                    Rating = ratingValue
                }
            });
        }

        [HttpPost("rate")]
        [Authorize]
        public async Task<IActionResult> RateBook([FromBody] RateBookDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = BuildModelStateMessage("Invalid rating payload.") });
            }

            if (dto.BookId <= 0)
            {
                return BadRequest(new { message = "A valid bookId is required." });
            }

            if (IsRatingInvalid(dto.Rating))
            {
                return BadRequest(new { message = BuildInvalidRatingMessage(dto.Rating) });
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "You must be logged in to rate books." });
            }

            var existingReview = await _repo.GetByUserAndBookAsync(userId, dto.BookId);
            var responseMessage = existingReview == null
                ? "Rating saved successfully."
                : "Rating updated successfully.";

            if (existingReview == null)
            {
                var newReview = new Review
                {
                    ApplicationUserId = userId,
                    BookId = dto.BookId,
                    Rating = dto.Rating,
                    CreatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(newReview);
            }
            else
            {
                existingReview.Rating = dto.Rating;
                await _repo.UpdateAsync(existingReview);
            }

            return Ok(new
            {
                message = responseMessage,
                data = new UserRatingDto
                {
                    BookId = dto.BookId,
                    Rating = dto.Rating
                }
            });
        }

        private string? GetUserId() =>
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.FindFirstValue("sub")
            ?? User?.FindFirstValue(ClaimTypes.Name);

        private string BuildModelStateMessage(string prefix)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();

            if (!errors.Any())
            {
                return prefix;
            }

            return $"{prefix} {string.Join(" ", errors)}";
        }

        private static string? NormalizeReviewText(string? text) =>
            string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        private static string BuildInvalidRatingMessage(int rating) =>
            $"Rating '{rating}' is invalid. Rating must be between 1 and 5.";

        private static object BuildReviewResponse(Review review) => new
        {
            review.ReviewId,
            review.BookId,
            review.Rating,
            review.ReviewText,
            review.CreatedAt,
            review.ApplicationUserId
        };

        private static bool IsRatingInvalid(int rating) => rating < 1 || rating > 5;
    }
}
