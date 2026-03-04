using MyApi.Model.Response;

namespace MyApi.Services.FinePayments
{
    public interface IFinePaymentService
    {
        Task<FinePaymentResponse> CreateFinePaymentAsync(Guid userId, int loanId, int amount, string? paymentMethod = null, string? description = null);
        Task<FinePaymentReportResponse> GetAllFinePaymentsAsync();
        Task<FinePaymentReportResponse> GetFinePaymentsByDateAsync(DateTime startDate, DateTime endDate);
        Task<FinePaymentReportResponse> GetFinePaymentsByWeekAsync(int week, int year);
        Task<FinePaymentReportResponse> GetFinePaymentsByMonthAsync(int month, int year);
        Task<FinePaymentReportResponse> GetFinePaymentsByYearAsync(int year);
        Task<FinePaymentReportResponse> GetUserFinePaymentsAsync(Guid userId);
        Task<FinePaymentStatisticsResponse> GetFinePaymentStatisticsAsync();
    }
}
