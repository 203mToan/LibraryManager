namespace MyApi.Entities
{
    public class Category : BaseEntity<int>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Book>? Books { get; set; }
    }
}
