namespace MyApi.Entities
{
    /// <summary>
    /// Payment entity ?? track t?t c? các giao d?ch VNPay
    /// </summary>
    public class Payment : BaseEntity<int>
    {
        /// <summary>
        /// OrderId t? VNPay request (unique identifier)
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// S? ti?n thanh toán (VND)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Tr?ng thái thanh toán: Pending, Success, Failed, Cancelled
        /// </summary>
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Cancelled

        /// <summary>
        /// ID c?a ng??i dùng thanh toán
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID c?a loan (m??n sách) liên quan
        /// </summary>
        public int? LoanId { get; set; }

        /// <summary>
        /// Transaction ID t? VNPay (khi thanh toán thành công)
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Mã ph?n h?i t? VNPay (00 = thành công)
        /// </summary>
        public string? VnPayResponseCode { get; set; }

        /// <summary>
        /// Th?i gian thanh toán thành công
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Th?i gian h?t h?n payment
        /// </summary>
        public DateTime? ExpiredAt { get; set; }

        /// <summary>
        /// Mô t? thanh toán
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Error message n?u thanh toán th?t b?i
        /// </summary>
        public string? ErrorMessage { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public Loan? Loan { get; set; }
    }
}
