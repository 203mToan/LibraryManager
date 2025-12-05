namespace MyApi.Model.Request
{
    public class CategoryUpdateRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
