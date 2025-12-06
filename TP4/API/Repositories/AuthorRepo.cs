using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ApplicationContext _context;

        public AuthorRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _context.Authors.ToListAsync();
        }

        public async Task<Author?> GetByIdAsync(int id)
        {
            return await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);
        }

        public async Task<Author> AddAsync(Author author)
        {
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            return author;
        }

        public async Task<bool> UpdateAsync(Author author)
        {
            _context.Authors.Update(author);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
{
    var author = await _context.Authors
        .Include(a => a.Books)
        .FirstOrDefaultAsync(a => a.AuthorId == id);

    if (author == null) return false;

    if (author.Books.Any())
        throw new InvalidOperationException("AUTHOR_IN_USE");

    _context.Authors.Remove(author);
    return await _context.SaveChangesAsync() > 0;
}


        public async Task<bool> HasBooksAsync(int authorId)
        {
            return await _context.Books.AnyAsync(b => b.AuthorId == authorId);
        }
    }
}
