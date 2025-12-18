namespace MyApi.Entities
{
    public class Author : BaseEntity<Guid>
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Nationality { get; set; }
        public int? BirthYear { get; set; }

        // Navigation
        public ICollection<Book>? Books { get; set; }
    }
}
