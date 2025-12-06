using API.DTO;
using API.Interfaces;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepo bookRepo;
        private readonly IImageService imageService;

        public BooksController(IBookRepo bookRepo, IImageService imageService)
        {
            this.bookRepo = bookRepo;
            this.imageService = imageService;
        }

        // GET ALL BOOKS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await bookRepo.GetAllAsync();
            var result = books.Select(b => new BookReadDto
            {
                BookId = b.BookId,
                Title = b.Title,
                AuthorId = b.AuthorId,
                GenreId = b.GenreId,
                PublishYear = b.PublishYear,
                Description = b.Description,
                ImageUrl = BuildPublicImageUrl(b.ImageUrl)
            });
            return Ok(result);
        }

        // GET BOOK BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await bookRepo.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { message = "Livre introuvable !" });
            }

            var result = new BookReadDto
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.AuthorId,
                GenreId = book.GenreId,
                PublishYear = book.PublishYear,
                Description = book.Description,
                ImageUrl = BuildPublicImageUrl(book.ImageUrl)
            };
            return Ok(result);
        }

        // CREATE BOOK
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BookCreateDto dto)
        {
            string? imageUrl = null;

            // Upload image if provided
            if (dto.Image != null)
            {
                try
                {
                    imageUrl = await imageService.UploadImageAsync(dto.Image, "books");
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Image upload failed. Please try again.", detail = ex.Message });
                }
            }

            var newBook = new Book
            {
                Title = dto.Title,
                AuthorId = dto.AuthorId,
                GenreId = dto.GenreId,
                PublishYear = dto.PublishYear,
                Description = dto.Description,
                ImageUrl = imageUrl
            };

            await bookRepo.CreateAsync(newBook);

            var responseDto = new BookReadDto
            {
                BookId = newBook.BookId,
                Title = newBook.Title,
                AuthorId = newBook.AuthorId,
                GenreId = newBook.GenreId,
                PublishYear = newBook.PublishYear,
                Description = newBook.Description,
                ImageUrl = BuildPublicImageUrl(newBook.ImageUrl)
            };

            return CreatedAtAction(nameof(GetById), new { id = newBook.BookId }, responseDto);
        }

        // UPDATE BOOK
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] BookCreateDto dto)
        {
            var book = await bookRepo.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { message = "Livre introuvable !" });
            }

            // Handle image upload/replacement
            if (dto.Image != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(book.ImageUrl))
                    {
                        imageService.DeleteImage(book.ImageUrl);
                    }

                    book.ImageUrl = await imageService.UploadImageAsync(dto.Image, "books");
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Image upload failed. Please try again.", detail = ex.Message });
                }
            }

            book.Title = dto.Title;
            book.AuthorId = dto.AuthorId;
            book.GenreId = dto.GenreId;
            book.PublishYear = dto.PublishYear;
            book.Description = dto.Description;

            await bookRepo.UpdateAsync(book);

            var updatedDto = new BookReadDto
            {
                BookId = book.BookId,
                Title = book.Title,
                AuthorId = book.AuthorId,
                GenreId = book.GenreId,
                PublishYear = book.PublishYear,
                Description = book.Description,
                ImageUrl = BuildPublicImageUrl(book.ImageUrl)
            };

            return Ok(updatedDto);
        }

        // DELETE BOOK
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await bookRepo.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { message = "Livre introuvable !" });
            }

            if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                imageService.DeleteImage(book.ImageUrl);
            }

            await bookRepo.DeleteAsync(id);
            return Ok(new { message = "Book deleted." });
        }

        private string? BuildPublicImageUrl(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absolute) &&
                (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                return absolute.ToString();
            }

            var request = HttpContext?.Request;
            if (request == null)
            {
                return imagePath;
            }

            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return new Uri(new Uri(baseUrl), imagePath.TrimStart('/')).ToString();
        }
    }
}
