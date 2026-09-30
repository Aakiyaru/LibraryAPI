namespace Library.Application.Dtos;

public class UpdateBookRequest
{
    public string Title { get; set; }
    public string ISBN { get; set; }
    public string Genre { get; set; }
    public int PublicationYear { get; set; }
    public int TotalCopies { get; set; }
}