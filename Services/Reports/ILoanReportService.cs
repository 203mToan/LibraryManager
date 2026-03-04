using MyApi.Model.Response;

namespace MyApi.Services.Reports
{
    public interface ILoanReportService
    {
        Task<LoanTrendResponse> GetLoanTrendAsync(string period, int year, int? month = null, int? week = null);
        Task<CategoryDistributionResponse> GetCategoryDistributionAsync(int year, int month);
        Task<TopUserResponse> GetTopUsersAsync(string period, int year, int? month = null);
        Task<TopBookResponse> GetTopBooksAsync(string period, int year, int? month = null);
    }
}
