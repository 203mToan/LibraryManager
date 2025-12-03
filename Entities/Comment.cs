namespace MyApi.Entities
{
    public class Comment : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }

        public int Rating { get; set; } // 1-5
        public string? Content { get; set; }
        public string? Status { get; set; } // Pending / Approved / Rejected

        // Navigation
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
