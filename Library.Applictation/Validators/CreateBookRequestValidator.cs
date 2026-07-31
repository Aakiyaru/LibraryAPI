using FluentValidation;
using Library.Application.Dtos;

namespace Library.Applictation.Validators
{
    public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
    {
        public CreateBookRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Название обязательно")
                .MaximumLength(200).WithMessage("Название не должно превышать 200 символов");

            RuleFor(x => x.ISBN)
                .NotEmpty().WithMessage("ISBN обязателен")
                .Length(10, 20).WithMessage("ISBN должен быть от 10 до 20 символов");

            RuleFor(x => x.Genre)
                .NotEmpty().WithMessage("Жанр обязателен");

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1450, DateTime.Now.Year)
                .WithMessage($"Год издания должен быть между 1450 и {DateTime.Now.Year}");

            RuleFor(x => x.TotalCopies)
                .GreaterThan(0).WithMessage("Количество экземпляров должно быть больше 0");
        }
    }
}
