using Library.Application.Dtos;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services;

public class LoanService : ILoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookLoanDto>> GetAllLoansAsync()
    {
        var loans = await _context.Loans
            .Include(l => l.User)
            .Include(l => l.Book)
            .ToListAsync();

        return loans.Select(MapToDto);
    }

    public async Task<IEnumerable<BookLoanDto>> GetUserLoansAsync(Guid userId)
    {
        var loans = await _context.Loans
            .Include(l => l.User)
            .Include(l => l.Book)
            .Where(l => l.UserId == userId)
            .ToListAsync();

        return loans.Select(MapToDto);
    }

    public async Task<BookLoanDto> BorrowBookAsync(BorrowRequest request)
    {
        // Получаем пользователя и книгу с проверкой существования
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден");

        var book = await _context.Books.FindAsync(request.BookId);
        if (book == null)
            throw new KeyNotFoundException("Книга не найдена");

        // Создаём запись о выдаче через фабричный метод (вся логика внутри)
        var loanDays = request.LoanDays ?? 14;
        var loan = BookLoan.Create(user, book, loanDays);

        // Уменьшаем количество доступных копий (это делает BookLoan.Create? 
        // Нет, мы вызываем BorrowCopy вручную, так как Create только создаёт запись, 
        // а уменьшение копий — это отдельная операция.
        book.BorrowCopy(); // уменьшаем AvailableCopies

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return MapToDto(loan);
    }

    public async Task<BookLoanDto> ReturnBookAsync(ReturnRequest request)
    {
        var loan = await _context.Loans
            .Include(l => l.User)
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == request.LoanId);

        if (loan == null)
            throw new KeyNotFoundException("Запись о выдаче не найдена");

        // Если уже возвращена — ошибка (обработается внутри метода Return)
        loan.Return(); // вызываем метод, который увеличит AvailableCopies и рассчитает штраф

        await _context.SaveChangesAsync();

        return MapToDto(loan);
    }

    private static BookLoanDto MapToDto(BookLoan loan)
    {
        return new BookLoanDto
        {
            Id = loan.Id,
            UserName = loan.User?.FullName ?? "Unknown",
            BookTitle = loan.Book?.Title ?? "Unknown",
            BorrowDate = loan.BorrowDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Fine = loan.Fine
        };
    }
}