using Library.Application.Dtos;
using Library.Applictation.Dtos;
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

        public async Task<PagedResult<BookDto>> GetBooksAsync(BookQueryParameters parameters)
        {
            var query = _context.Books.AsQueryable();

            if(!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                query = query.Where(b => b.Title.Contains(parameters.SearchTerm));
            }

            if(!string.IsNullOrWhiteSpace(parameters.Genre))
            {
                query = query.Where(b => b.Genre == parameters.Genre);
            }

            if(parameters.MinYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear >= parameters.MinYear.Value);
            }

            if(parameters.MaxYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear <= parameters.MaxYear.Value);
            }

            if(!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "title" => parameters.SortDescending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                    "year" => parameters.SortDescending ? query.OrderByDescending(b => b.PublicationYear) : query.OrderBy(b => b.PublicationYear),
                    "genre" => parameters.SortDescending ? query.OrderByDescending(b => b.Genre) : query.OrderBy(b => b.Genre),
                    _ => query.OrderBy(b => b.Title)
                };
            }
            else
            {
                query = query.OrderBy(b => b.Title);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    Genre = b.Genre,
                    PublicationYear = b.PublicationYear,
                    TotalCopies = b.TotalCopies,
                    AvailableCopies = b.AvailableCopies
                })
                .ToListAsync();

            return new PagedResult<BookDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            };
        }
    }
}
