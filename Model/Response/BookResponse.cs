namespace MyApi.Model.Response
{
    public class BookResponse
    {
        public int Id { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public Guid AuthorId { get; set; }
        public int CategoryId { get; set; }

        public string? Publisher { get; set; }
        public int? YearPublished { get; set; }
        public int StockQuantity { get; set; } = 0;
    }

    public class PagedBookResponse : PagedHttpResponse<BookResponse>
    {
        public PagedBookResponse(IEnumerable<BookResponse> items, int totalItems, int? pageSize)
        {
            TotalItems = totalItems;
            TotalPages = PaginationUtils.TotalPagesConversion(totalItems, pageSize);
            Items = items;
        }
    }
}
