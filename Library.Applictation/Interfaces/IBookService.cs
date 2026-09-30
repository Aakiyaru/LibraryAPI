using Library.Application.Dtos;
using Library.Applictation.Dtos;

namespace Library.Application.Interfaces;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetBooksAsync(BookQueryParameters parameters);
    Task<BookDto> GetBookByIdAsync(Guid id);
    Task<BookDto> CreateBookAsync(CreateBookRequest request);
    Task<BookDto> UpdateBookAsync(Guid id, UpdateBookRequest request);
    Task DeleteBookAsync(Guid id);
    Task RestoreBookAsync(Guid id);
}