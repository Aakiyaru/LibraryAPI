namespace Library.Domain.Entities;

public class Book
{
    public Guid Id { get; private set; } // private set чтобы нельзя было изменить ID извне
    public string Title { get; private set; }
    public string ISBN { get; private set; } // Уникальный номер
    public string Genre { get; private set; }
    public int PublicationYear { get; private set; }
    public int TotalCopies { get; private set; } // Всего экземпляров в библиотеке
    public int AvailableCopies { get; private set; } // Сколько свободно сейчас
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Book() { } // Для EF

    // Фабричный метод для создания книги
    public static Book Create(string title, string isbn, string genre, int year, int copies)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Название не может быть пустым", nameof(title));
        }

        if (copies <= 0)
        {
            throw new ArgumentException("Количество экземпляров должно быть больше 0", nameof(copies));
        }

        return new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            ISBN = isbn,
            Genre = genre,
            PublicationYear = year,
            TotalCopies = copies,
            AvailableCopies = copies // Изначально все книги доступны
        };
    }

    //выдать книгу
    public void BorrowCopy()
    {
        if (AvailableCopies <= 0)
        {
            throw new InvalidOperationException("Нет доступных экземпляров книги");
        }

        AvailableCopies--;
    }

    //вернуть книгу
    public void ReturnCopy()
    {
        if (AvailableCopies >= TotalCopies)
        {
            throw new InvalidOperationException("Все книги уже на месте");
        }

        AvailableCopies++;
    }

    public void UpdateDetails(string title, string isbn, string genre, int year, int totalCopies)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название не может быть пустым", nameof(title));
        if (totalCopies < AvailableCopies)
            throw new InvalidOperationException(
                "Нельзя уменьшить общее количество ниже уже выданных экземпляров");

        var borrowed = TotalCopies - AvailableCopies;
        if (totalCopies < borrowed)
            throw new InvalidOperationException(
                $"Нельзя установить общее количество {totalCopies}: " +
                $"у читателей уже {borrowed} экземпляров");

        Title = title;
        ISBN = isbn;
        Genre = genre;
        PublicationYear = year;

        AvailableCopies += totalCopies - TotalCopies;
        TotalCopies = totalCopies;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Книга уже удалена");
        if (AvailableCopies < TotalCopies)
            throw new InvalidOperationException(
                "Нельзя удалить книгу, пока не возвращены все выданные экземпляры");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Книга не была удалена");
        IsDeleted = false;
        DeletedAt = null;
    }
}