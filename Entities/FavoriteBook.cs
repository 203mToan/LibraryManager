namespace MyApi.Entities
{
    public class FavoriteBook : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
