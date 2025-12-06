using System;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class BookRepo : IBookRepo
    {
        private readonly ApplicationContext _context;
        public BookRepo(ApplicationContext context) => _context = context;

        public async Task<IEnumerable<Book>> GetAllAsync() =>
            await _context.Books.Include(b => b.Author).Include(b => b.Genre).ToListAsync();

        public async Task<Book?> GetByIdAsync(int id) =>
            await _context.Books.Include(b => b.Author).Include(b => b.Genre)
                     .FirstOrDefaultAsync(b => b.BookId == id);

        public async Task<Book> CreateAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
{
    var book = await _context.Books
        .Include(b => b.Reviews)
        .Include(b => b.ReadingLists)
        .FirstOrDefaultAsync(b => b.BookId == id);

    if (book == null) return;

    if (book.Reviews.Any())
        throw new InvalidOperationException("BOOK_HAS_REVIEWS");

    _context.Books.Remove(book);
    await _context.SaveChangesAsync();
}

    }

}
