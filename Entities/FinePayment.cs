namespace MyApi.Entities
{
    public class FinePayment : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public int LoanId { get; set; }
        public int Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentMethod { get; set; } // Cash, Card, Online, etc.
        public string? Description { get; set; }
        public User User { get; set; } = null!;
        public Loan Loan { get; set; } = null!;
    }
}
