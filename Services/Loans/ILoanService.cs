using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.Loans
{
    public interface ILoanService
    {
        Task<LoanResponse> CreateLoanAsync(LoanRequest request);
        Task<List<LoanResponse>> GetAllLoansAsync();
        Task<LoanResponse?> GetLoanByIdAsync(int id);
        Task<LoanResponse?> ReturnBookAsync(int id, DateTime returnDate);
        Task<List<LoanResponse>> GetMyLoansAsync();
        Task<LoanResponse?> ApproveLoanAsync(int loanId);
        Task<LoanResponse?> PayFineAsync(int loanId);


    }
}
