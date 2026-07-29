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
}