using API.Dtos.Author;
using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorRepository _repo;

        public AuthorController(IAuthorRepository repo)
        {
            _repo = repo;
        }

        // GET api/author
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var authors = await _repo.GetAllAsync();

            var result = authors.Select(a => new AuthorReadDto
            {
                AuthorId = a.AuthorId,
                Name = a.Name,
                Bio = a.Bio
            });

            return Ok(result);
        }

        // GET api/author/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var author = await _repo.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound(new { message = "Author not found." });
            }

            var dto = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Bio = author.Bio
            };

            return Ok(dto);
        }

        // POST api/author
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(AuthorCreateDto dto)
        {
            var author = new Author
            {
                Name = dto.Name,
                Bio = dto.Bio
            };

            await _repo.AddAsync(author);

            var responseDto = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Bio = author.Bio
            };

            return CreatedAtAction(nameof(GetById), new { id = author.AuthorId }, new
            {
                message = "Author created successfully.",
                data = responseDto
            });
        }

        // PUT api/author/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Update(int id, AuthorUpdateDto dto)
        {
            var author = await _repo.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound(new { message = "Author not found." });
            }

            author.Name = dto.Name;
            author.Bio = dto.Bio;

            var updated = await _repo.UpdateAsync(author);
            if (!updated)
            {
                return StatusCode(500, new { message = "Unable to update author. Please try again." });
            }

            var responseDto = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Bio = author.Bio
            };

            return Ok(new
            {
                message = "Author updated successfully.",
                data = responseDto
            });
        }

        // DELETE api/author/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {

            var author = await _repo.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound(new { message = "Author not found." });
            }

            if (await _repo.HasBooksAsync(id))
            {
                return BadRequest(new { message = "You cannot delete this author because they are used in one or more books." });
            }

            try
            {
                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, new { message = "Unable to delete author. Please try again." });
                }

                return Ok(new
                {
                    message = "Author deleted successfully.",
                    data = new { id = author.AuthorId }
                });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "You cannot delete this author because they are used in one or more books." });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Unable to delete author because it is referenced by existing books." });
            }
        }
    }
}
