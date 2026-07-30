using Library.Application.Dtos;
using Library.Applictation.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _context;

        public BookService (AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _context.Books.ToListAsync();
            return books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Genre = b.Genre,
                PublicationYear = b.PublicationYear,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
            });
        }

        public async Task<BookDto> GetBookByIdAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return null;
            }

            return new BookDto
            {
                Id = id,
                Title = book.Title,
                ISBN = book.ISBN,
                Genre = book.Genre,
                PublicationYear = book.PublicationYear,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies
            };
        }

        public async Task<BookDto> CreateBookAsync(CreateBookRequest request)
        {
            var book = Book.Create(
                    request.Title,
                    request.ISBN,
                    request.Genre,
                    request.PublicationYear,
                    request.TotalCopies
                );

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                ISBN = book.ISBN,
                Genre = book.Genre,
                PublicationYear = book.PublicationYear,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies
            };
        }
    }
}
