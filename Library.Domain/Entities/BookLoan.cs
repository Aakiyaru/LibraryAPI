using Microsoft.VisualBasic;

namespace Library.Domain.Entities
{
    public class BookLoan
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public Guid BookId { get; private set; }
        public Book Book { get; private set; }
        public DateTime BorrowDate {  get; private set; }
        public DateTime DueDate {  get; private set; } //плановая дата возврата
        public DateTime? ReturnDate { get; private set; } //фактическая дата возврата, null если не возвращена
        public decimal? Fine { get; private set; } //штраф

        private BookLoan() { }

        public static BookLoan Create(User user, Book book, int loanDays = 14)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if(book == null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if(book.AvailableCopies <= 0)
            {
                throw new InvalidOperationException("Нет доступных экземпляров книги");
            }

            if(!user.CanBorrowMore())
            {
                throw new InvalidOperationException("Пользователь уже взял максимум книг (5)");
            }

            return new BookLoan
            {
                Id = Guid.NewGuid(),
                User = user,
                UserId = user.Id,
                Book = book,
                BookId = book.Id,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(loanDays)
            };
        }

        //метод возврата с учётом штрафа
        public void Return(decimal? finePerDay = null)
        {
            if (ReturnDate.HasValue)
            {
                throw new InvalidOperationException("Книга уже возвращена");
            }

            ReturnDate = DateTime.UtcNow;
            Book.ReturnCopy();

            if (ReturnDate > DueDate)
            {
                var daysOverude = (ReturnDate.Value - DueDate).Days;
                //Если не указано, используем 10 рублей в день
                var rate = finePerDay ?? 10m;
                Fine = daysOverude * rate;
            }
            else
            {
                Fine = 0;
            }
        }
    }
}
