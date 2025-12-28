namespace MyApi.Model.Response
{
    public class FavoriteBookResponse
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? ThumbnailUrl { get; set; }

        public string AuthorName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;

        public int? YearPublished { get; set; }
        public int StockQuantity { get; set; }
        public string? Publisher { get; set; }
        public bool IsFavorited { get; set; }
    }
}
