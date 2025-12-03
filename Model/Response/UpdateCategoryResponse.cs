namespace MyApi.Model.Response
{
    public class UpdateCategoryResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
