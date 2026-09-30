using FluentAssertions;
using Library.Domain.Entities;

namespace Library.Tests.Domain;

public class BookTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnBook()
    {
        // Act
        var book = Book.Create("Война и мир", "978-5-17-118636-4", "Роман", 1869, 5);

        // Assert
        book.Title.Should().Be("Война и мир");
        book.TotalCopies.Should().Be(5);
        book.AvailableCopies.Should().Be(5);   // все доступны изначально
        book.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldThrow(string title)
    {
        var act = () => Book.Create(title, "isbn", "genre", 2000, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroCopies_ShouldThrow()
    {
        var act = () => Book.Create("Title", "isbn", "genre", 2000, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BorrowCopy_WhenAvailable_ShouldDecrementAvailable()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);

        book.BorrowCopy();

        book.AvailableCopies.Should().Be(2);
        book.TotalCopies.Should().Be(3); // Total не меняется
    }

    [Fact]
    public void BorrowCopy_WhenNoAvailable_ShouldThrow()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 1);
        book.BorrowCopy(); // теперь 0

        var act = () => book.BorrowCopy();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*доступных экземпляров*");
    }

    [Fact]
    public void ReturnCopy_ShouldIncrementAvailable()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);
        book.BorrowCopy();

        book.ReturnCopy();

        book.AvailableCopies.Should().Be(3);
    }

    [Fact]
    public void ReturnCopy_WhenAllReturned_ShouldThrow()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);
        // ничего не брали, но пытаемся вернуть

        var act = () => book.ReturnCopy();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateDetails_WithNewTotalCopies_ShouldRecomputeAvailable()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);
        book.BorrowCopy(); // Available = 2

        book.UpdateDetails("New Title", "new-isbn", "new-genre", 2001, 5);

        book.TotalCopies.Should().Be(5);
        book.AvailableCopies.Should().Be(4); // 2 + (5 - 3)
    }

    [Fact]
    public void UpdateDetails_WhenNewTotalLowerThanBorrowed_ShouldThrow()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 5);
        book.BorrowCopy();
        book.BorrowCopy();
        book.BorrowCopy();
        book.BorrowCopy(); // Available = 1, borrowed = 4

        var act = () => book.UpdateDetails("T", "i", "g", 2000, 2); // 2 < 4
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsDeleted_WhenBooksOut_ShouldThrow()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);
        book.BorrowCopy();

        var act = () => book.MarkAsDeleted();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsDeleted_WhenAllReturned_ShouldSetFlag()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);

        book.MarkAsDeleted();

        book.IsDeleted.Should().BeTrue();
        book.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_AfterDelete_ShouldClearFlag()
    {
        var book = Book.Create("Title", "isbn", "genre", 2000, 3);
        book.MarkAsDeleted();

        book.Restore();

        book.IsDeleted.Should().BeFalse();
        book.DeletedAt.Should().BeNull();
    }
}