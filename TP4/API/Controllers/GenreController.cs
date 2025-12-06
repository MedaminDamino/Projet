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
    public class GenreController : ControllerBase
    {
        private readonly IGenreRepository _repo;

        public GenreController(IGenreRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var genres = await _repo.GetAllAsync();
            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var genre = await _repo.GetByIdAsync(id);
            if (genre == null)
                return NotFound(new { message = "Genre not found." });

            return Ok(genre);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create(Genre genre)
        {
            var newGenre = await _repo.AddAsync(genre);
            return CreatedAtAction(nameof(GetById), new { id = newGenre.GenreId }, new
            {
                message = "Genre created successfully.",
                data = newGenre
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Update(int id, Genre genre)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Genre not found." });

            existing.GenreName = genre.GenreName;

            await _repo.UpdateAsync(existing);

            return Ok(new
            {
                message = "Genre updated successfully.",
                data = existing
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {

            var genre = await _repo.GetByIdAsync(id);
            if (genre == null)
            {
                return NotFound(new { message = "Genre not found." });
            }

            if (await _repo.HasBooksAsync(id))
            {
                return BadRequest(new { message = "You cannot delete this genre because it is used by one or more books." });
            }

            try
            {
                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                    return StatusCode(500, new { message = "Unable to delete genre. Please try again." });

                return Ok(new
                {
                    message = "Genre deleted successfully.",
                    data = new { id = genre.GenreId }
                });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "You cannot delete this genre because it is used by one or more books." });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Unable to delete genre because it is referenced by existing books." });
            }
        }
    }
}
