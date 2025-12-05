using API.Dtos.Author;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

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
            if (author == null) return NotFound();

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
        public async Task<IActionResult> Create(AuthorCreateDto dto)
        {
            var author = new Author
            {
                Name = dto.Name,
                Bio = dto.Bio
            };

            await _repo.AddAsync(author);

            return CreatedAtAction(nameof(GetById), new { id = author.AuthorId }, author);
        }

        // PUT api/author/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AuthorUpdateDto dto)
        {
            var author = await _repo.GetByIdAsync(id);
            if (author == null) return NotFound();

            author.Name = dto.Name;
            author.Bio = dto.Bio;

            await _repo.UpdateAsync(author);

            return NoContent();
        }

        // DELETE api/author/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
