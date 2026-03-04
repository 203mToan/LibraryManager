using MyApi.Entities;

namespace MyApi.Services.Payments
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(Guid userId, int? loanId, decimal amount, string orderInfo, DateTime? expiredAt);
        Task<Payment> UpdatePaymentPendingAsync(string orderId);
        Task<Payment> UpdatePaymentSuccessAsync(string orderId, string transactionId, string vnPayResponseCode);
        Task<Payment> UpdatePaymentFailedAsync(string orderId, string errorMessage);
        Task<Payment?> GetPaymentByOrderIdAsync(string orderId);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<List<Payment>> GetUserPaymentsAsync(Guid userId);
        Task<List<Payment>> GetLoanPaymentsAsync(int loanId);
        Task<bool> IsPaymentSuccessAsync(string orderId);
        Task<decimal> GetLoanFineAmountAsync(int loanId);
        Task<bool> HasUnpaidFineAsync(int loanId);
        Task<(bool IsValid, string Message)> ValidatePaymentAmountAsync(int loanId, decimal paymentAmount);
    }
}
