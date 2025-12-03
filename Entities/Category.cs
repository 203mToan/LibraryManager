namespace MyApi.Entities
{
    public class Category : BaseEntity<int>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        // Navigation
        public ICollection<Book>? Books { get; set; }
    }
}
