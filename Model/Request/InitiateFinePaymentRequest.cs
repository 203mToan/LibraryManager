namespace MyApi.Model.Request
{
    public class InitiateFinePaymentRequest
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string? Language { get; set; } = "vn";
        public string? BankCode { get; set; }
    }
}
