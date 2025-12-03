namespace MyApi.Model.Request
{
    public class BookCreateRequest
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }

        public Guid AuthorId { get; set; }
        public Guid CategoryId { get; set; }

        public string? Publisher { get; set; }
        public int? YearPublished { get; set; }
        public int StockQuantity { get; set; } = 0;
    }
}
