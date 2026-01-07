using MyApi.Model.Response;

namespace MyApi.Services.FinePayments
{
    public interface IFinePaymentService
    {
        // Create
        Task<FinePaymentResponse> CreateFinePaymentAsync(Guid userId, int loanId, int amount, string? paymentMethod = null, string? description = null);

        // Read - Get all
        Task<FinePaymentReportResponse> GetAllFinePaymentsAsync();

        // Read - Filter by period
        Task<FinePaymentReportResponse> GetFinePaymentsByDateAsync(DateTime startDate, DateTime endDate);
        Task<FinePaymentReportResponse> GetFinePaymentsByWeekAsync(int week, int year);
        Task<FinePaymentReportResponse> GetFinePaymentsByMonthAsync(int month, int year);
        Task<FinePaymentReportResponse> GetFinePaymentsByYearAsync(int year);

        // Read - Filter by user
        Task<FinePaymentReportResponse> GetUserFinePaymentsAsync(Guid userId);

        // Statistics
        Task<FinePaymentStatisticsResponse> GetFinePaymentStatisticsAsync();
    }
}
