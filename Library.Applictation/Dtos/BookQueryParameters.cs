namespace Library.Applictation.Dtos
{
    public class BookQueryParameters
    {
        public string? SearchTerm { get; set; } //по названию или автору
        public string? Genre { get; set; }
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
