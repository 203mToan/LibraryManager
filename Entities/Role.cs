namespace MyApi.Entities
{
    public class Role : BaseEntity<int>
    {
        public string Name { get; set; } = null!; 
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
