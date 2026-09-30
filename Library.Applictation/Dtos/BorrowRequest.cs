namespace Library.Application.Dtos;

public class BorrowRequest
{
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }
    public int? LoanDays { get; set; } // опционально, по умолчанию 14
}