namespace MyApi.Model.Response
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? BookCount { get; set; }
    }
}

