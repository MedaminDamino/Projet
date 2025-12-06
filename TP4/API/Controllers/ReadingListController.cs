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
    public class ReadingListController : ControllerBase
    {
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
            public string ErrorCode { get; set; }
        }

        public class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string ErrorCode { get; set; }
        }
        private readonly IReadingListRepo _repo;
        private static readonly string[] AllowedStatuses =
        {
            "NotStarted",
            "Reading",
            "Completed"
        };

        public ReadingListController(IReadingListRepo repo)
        {
            _repo = repo;
        }

        private bool IsValidStatus(string status)
        {
            return AllowedStatuses.Contains(status);
        }

        private ReadingListReadDto MapToReadDto(ReadingList list)
        {
            return new ReadingListReadDto
            {
                ReadingListID = list.ReadingListID,
                ApplicationUserId = list.ApplicationUserId,
                BookId = list.BookId,
                Status = list.Status,
                AddedAt = list.AddedAt,
                Book = list.Book != null ? new BookReadDto
                {
                    BookId = list.Book.BookId,
                    Title = list.Book.Title,
                    AuthorId = list.Book.AuthorId,
                    GenreId = list.Book.GenreId,
                    PublishYear = list.Book.PublishYear,
                    Description = list.Book.Description,
                    ImageUrl = list.Book.ImageUrl
                } : null
            };
        }

        private async Task<bool> UserHasBookAsync(string userId, int bookId, int? excludeReadingListId = null)
        {
            var items = await _repo.GetAllAsync();
            return items.Any(item =>
                item.ApplicationUserId == userId &&
                item.BookId == bookId &&
                item.ReadingListID != excludeReadingListId);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repo.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<ReadingListReadDto>>
            {
                Success = true,
                Message = "Reading list retrieved successfully",
                Data = items.Select(MapToReadDto)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null)
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "The requested reading list entry was not found.",
                    ErrorCode = "READING_LIST_NOT_FOUND"
                });
            return Ok(new ApiResponse<ReadingListReadDto>
            {
                Success = true,
                Message = "Reading list entry retrieved successfully",
                Data = MapToReadDto(item)
            });
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Create(ReadingListCreateDto createDto)
        {
            if (!IsValidStatus(createDto.Status))
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid status value. Please use one of the following: NotStarted, Reading, or Completed.",
                    ErrorCode = "INVALID_STATUS"
                });

            // Check if user already has this book (exclude null since this is a new entry)
            if (await UserHasBookAsync(createDto.ApplicationUserId, createDto.BookId, excludeReadingListId: -1))
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "This book is already in your reading list. You can update the existing entry to change its status instead.",
                    ErrorCode = "BOOK_ALREADY_EXISTS"
                });

            var list = new ReadingList
            {
                ApplicationUserId = createDto.ApplicationUserId,
                BookId = createDto.BookId,
                Status = createDto.Status,
                AddedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(list);
            return CreatedAtAction(nameof(GetById), new { id = list.ReadingListID }, new ApiResponse<ReadingListReadDto>
            {
                Success = true,
                Message = "Book added to your reading list successfully",
                Data = MapToReadDto(list)
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> Update(int id, ReadingListUpdateDto updateDto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "The reading list entry you are trying to update was not found.",
                    ErrorCode = "READING_LIST_NOT_FOUND"
                });

            if (!IsValidStatus(updateDto.Status))
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid status value. Please use one of the following: NotStarted, Reading, or Completed.",
                    ErrorCode = "INVALID_STATUS"
                });

            // Check if user is trying to change to a book they already have (excluding current entry)
            if (existing.BookId != updateDto.BookId &&
                await UserHasBookAsync(updateDto.ApplicationUserId, updateDto.BookId, id))
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "This book is already in your reading list. You cannot add the same book twice.",
                    ErrorCode = "BOOK_ALREADY_EXISTS"
                });

            existing.Status = updateDto.Status;
            existing.BookId = updateDto.BookId;
            existing.ApplicationUserId = updateDto.ApplicationUserId;

            await _repo.UpdateAsync(existing);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Your reading list entry has been updated successfully",
                ErrorCode = null
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            // Note: Admin role is excluded - only SuperAdmin and User (for their own items) can delete

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "The reading list entry you are trying to delete was not found.",
                    ErrorCode = "READING_LIST_NOT_FOUND"
                });
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Reading list entry has been removed successfully",
                ErrorCode = null
            });
        }
    }
}
