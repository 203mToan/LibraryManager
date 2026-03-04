namespace MyApi.Entities
{
    public class Notification : BaseEntity<int>
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!; // LoanApproved, PaymentApproved, PaymentSuccess, etc.
        public int? LoanId { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation
        public User User { get; set; } = null!;
        public Loan? Loan { get; set; }
    }

    public enum NotificationType
    {
        LoanApproved,      
        PaymentApproved,   
        PaymentSuccess,    
        LoanOverdue,       
        PaymentRequested   
    }
}