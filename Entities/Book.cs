namespace MyApi.Entities
{
    public class Book : BaseEntity<int>
    {
        public string? ThumbnailUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public Guid AuthorId { get; set; } 
        public int? CategoryId { get; set; }

        public string? Publisher { get; set; }
        public int? YearPublished { get; set; }
        public int StockQuantity { get; set; } = 0;

        // Navigation
        public Author Author { get; set; } = null!;
        public Category Category { get; set; } = null!;

        public ICollection<Loan>? Loans { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<FavoriteBook>? FavoriteBooks { get; set; }

    }
}
