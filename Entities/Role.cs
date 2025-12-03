namespace MyApi.Entities
{
    public class Role : BaseEntity<int>
    {
        public string Name { get; set; } = null!; // UNIQUE, NOT NULL
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
