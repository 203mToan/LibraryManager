namespace MyApi.Entities
{
    public class Loan : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public int BookId { get; set; }

        public DateTime? LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string? Status { get; set; }  // Pending / Approved / Returned / Overdue

        // Navigation
        public User User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}
