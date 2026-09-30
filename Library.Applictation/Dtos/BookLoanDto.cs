namespace Library.Application.Dtos;

public class BookLoanDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string BookTitle { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal? Fine { get; set; }
    public bool IsActive => ReturnDate == null;
}