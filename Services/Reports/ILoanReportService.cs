using MyApi.Model.Response;

namespace MyApi.Services.Reports
{
    public interface ILoanReportService
    {
        // Xu h??ng m??n sách theo period
        Task<LoanTrendResponse> GetLoanTrendAsync(string period, int year, int? month = null, int? week = null);

        // Phân b? sách theo th? lo?i
        Task<CategoryDistributionResponse> GetCategoryDistributionAsync(int year, int month);

        // Top 5 ng??i dùng tích c?c
        Task<TopUserResponse> GetTopUsersAsync(string period, int year, int? month = null);

        // Top 5 sách hot
        Task<TopBookResponse> GetTopBooksAsync(string period, int year, int? month = null);
    }
}
