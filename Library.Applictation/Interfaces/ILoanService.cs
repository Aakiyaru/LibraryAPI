using Library.Application.Dtos;

namespace Library.Application.Interfaces;

public interface ILoanService
{
    Task<IEnumerable<BookLoanDto>> GetAllLoansAsync();
    Task<BookLoanDto> BorrowBookAsync(BorrowRequest request);
    Task<BookLoanDto> ReturnBookAsync(ReturnRequest request);
    Task<IEnumerable<BookLoanDto>> GetUserLoansAsync(Guid userId);
}