namespace MyApi.Model.Request
{
    /// <summary>
    /// Request ?? kh?i t?o VNPay payment cho ti?n ph?t
    /// </summary>
    public class InitiateFinePaymentRequest
    {
        /// <summary>
        /// Loan ID (m??n sách ID có ti?n ph?t)
        /// </summary>
        public int LoanId { get; set; }

        /// <summary>
        /// S? ti?n thanh toán (ph?i = FineAmount)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Ngôn ng? (vn/en)
        /// </summary>
        public string? Language { get; set; } = "vn";

        /// <summary>
        /// Mã ngân hàng (optional - n?u mu?n ch? ??nh ngân hàng)
        /// </summary>
        public string? BankCode { get; set; }
    }
}
