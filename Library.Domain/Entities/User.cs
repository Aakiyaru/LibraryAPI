using System.Text.RegularExpressions;

namespace Library.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public DateTime RegistrationDate { get; private set; }

        //навигационное совйство для истории выдач
        private readonly List<BookLoan> _loans = new();
        public IReadOnlyCollection<BookLoan> Loans => _loans.AsReadOnly();

        private User() { }

        public static User Create(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Имя не может быть пустым", nameof(fullName));
            }

            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Неверный формат email", nameof(email));
            }

            return new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                RegistrationDate = DateTime.UtcNow,
            };
        }

        private static bool IsValidEmail(string email)
        {
            //простая валидация, можно использовать Regex
            return email.Contains('@') && email.Contains('.');
        }

        public bool CanBorrowMore()
        {
            return Loans.Count(l => l.ReturnDate == null) < 5;
        }
    }
}
