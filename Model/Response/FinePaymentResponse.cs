namespace MyApi.Model.Response
{
    public class FinePaymentResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public int LoanId { get; set; }
        public string? BookName { get; set; }
        public int Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Description { get; set; }
    }

    public class FinePaymentReportResponse
    {
        public int TotalPayments { get; set; }
        public int TotalAmount { get; set; }
        public List<FinePaymentResponse> Payments { get; set; } = new();
    }

    public class FinePaymentStatisticsResponse
    {
        public int DailyTotal { get; set; }
        public int WeeklyTotal { get; set; }
        public int MonthlyTotal { get; set; }
        public int YearlyTotal { get; set; }
        public List<FinePaymentByDateResponse> ByDate { get; set; } = new();
    }

    public class FinePaymentByDateResponse
    {
        public string Date { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
    }

    public class FinePaymentSummaryResponse
    {
        public int TotalFinePayments { get; set; }
        public int TotalFineAmount { get; set; }
        public int TotalLoans { get; set; }
        public int OverdueLoans { get; set; }
    }

    public class LoanSummaryResponse
    {
        public int TotalLoans { get; set; }
        public int ApprovingLoans { get; set; }
        public int OverdueLoans { get; set; }
    }
}
