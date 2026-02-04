using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.Loans
{
    public interface ILoanService
    {
        Task<LoanResponse> CreateLoanAsync(LoanRequest request);
        Task<List<LoanResponse>> GetAllLoansAsync();
        Task<PagedHttpResponse<LoanResponse>> GetAllLoansPagedAsync(int? pageIndex, int? pageSize, string? status = null);
        Task<LoanResponse?> GetLoanByIdAsync(int id);
        Task<LoanResponse?> ReturnBookAsync(int id, DateTime returnDate);
        Task<List<LoanResponse>> GetMyLoansAsync();
        Task<PagedHttpResponse<LoanResponse>> GetMyLoansPagedAsync(int? pageIndex, int? pageSize, string? status = null);
        Task<LoanResponse?> ApproveLoanAsync(int loanId);
        Task<LoanResponse?> CancelLoanAsync(int loanId);
        Task<LoanResponse?> PayFineAsync(int loanId);
        Task<LoanResponse?> ApprovePaymentAsync(int loanId);
        Task<LoanSummaryResponse> GetLoanSummaryByPeriodAsync(int year, int? month = null);
    }
}
