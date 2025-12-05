using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

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
                return NotFound();

            return Ok(genre);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Genre genre)
        {
            var newGenre = await _repo.AddAsync(genre);
            return CreatedAtAction(nameof(GetById), new { id = newGenre.GenreId }, newGenre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Genre genre)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.GenreName = genre.GenreName;

            await _repo.UpdateAsync(existing);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
