namespace MyApi.Entities
{
    public class Payment : BaseEntity<int>
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Cancelled
        public Guid UserId { get; set; }
        public int? LoanId { get; set; }
        public string? TransactionId { get; set; }
        public string? VnPayResponseCode { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? Description { get; set; }
        public string? ErrorMessage { get; set; }
        public User User { get; set; } = null!;
        public Loan? Loan { get; set; }
    }
}
