using Library.Application.Dtos;
using Library.Applictation.Dtos;

namespace Library.Applictation.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(Guid id);
        Task<BookDto> CreateBookAsync(CreateBookRequest request);
        Task<PagedResult<BookDto>> GetBooksAsync(BookQueryParameters parameters);
    }
}
