namespace MyApi
{
    public class PagedHttpResponse<TResponse>
    {
        public int? TotalItems { get; set; }

        public int? TotalPages { get; set; }

        public IEnumerable<TResponse> Items { get; set; }
    }
}
