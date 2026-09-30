using FluentAssertions;
using Library.Domain.Entities;

namespace Library.Tests.Domain;

public class BookLoanTests
{
    private static (User user, Book book) CreateValidPair(int copies = 3)
    {
        var user = User.Create("Иван", "ivan@test.com", "hash");
        var book = Book.Create("Война и мир", "isbn", "Роман", 1869, copies);
        return (user, book);
    }

    [Fact]
    public void Create_WithValidData_ShouldSetDueDate()
    {
        var (user, book) = CreateValidPair();

        var loan = BookLoan.Create(user, book, loanDays: 14);

        loan.BorrowDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        loan.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(14), TimeSpan.FromSeconds(2));
        loan.ReturnDate.Should().BeNull();
        loan.Fine.Should().BeNull();
    }

    [Fact]
    public void Return_WithoutOverdue_ShouldSetZeroFine()
    {
        var (user, book) = CreateValidPair();
        var loan = BookLoan.Create(user, book, loanDays: 14);

        loan.Return();

        loan.ReturnDate.Should().NotBeNull();
        loan.Fine.Should().Be(0);
        book.AvailableCopies.Should().Be(3); // не забыл вернуть копию
    }

    [Fact]
    public void Return_WithOverdue_ShouldCalculateFine()
    {
        var (user, book) = CreateValidPair();
        var loan = BookLoan.Create(user, book, loanDays: 0); // просрочка сразу

        // Ждём немного, чтобы ReturnDate > DueDate
        Thread.Sleep(1100);

        loan.Return(finePerDay: 10m);

        loan.Fine.Should().BeGreaterThan(0);
        book.AvailableCopies.Should().Be(3);
    }

    [Fact]
    public void Return_Twice_ShouldThrow()
    {
        var (user, book) = CreateValidPair();
        var loan = BookLoan.Create(user, book);
        loan.Return();

        var act = () => loan.Return();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже возвращена*");
    }
}