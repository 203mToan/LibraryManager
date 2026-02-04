using MyApi.Entities;

namespace MyApi.Services.Payments
{
    /// <summary>
    /// Service ?? qu?n lý Payment transactions
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// T?o m?i payment record
        /// </summary>
        Task<Payment> CreatePaymentAsync(Guid userId, int? loanId, decimal amount, string orderInfo, DateTime? expiredAt);

        /// <summary>
        /// C?p nh?t status payment thành Pending
        /// </summary>
        Task<Payment> UpdatePaymentPendingAsync(string orderId);

        /// <summary>
        /// C?p nh?t payment thành Success
        /// ? T? ??ng: Update Loan Status, Reset FineAmount, T?o FinePayment record
        /// </summary>
        Task<Payment> UpdatePaymentSuccessAsync(string orderId, string transactionId, string vnPayResponseCode);

        /// <summary>
        /// C?p nh?t payment thành Failed
        /// </summary>
        Task<Payment> UpdatePaymentFailedAsync(string orderId, string errorMessage);

        /// <summary>
        /// L?y payment by OrderId
        /// </summary>
        Task<Payment?> GetPaymentByOrderIdAsync(string orderId);

        /// <summary>
        /// L?y payment by Id
        /// </summary>
        Task<Payment?> GetPaymentByIdAsync(int paymentId);

        /// <summary>
        /// L?y danh sách payment c?a user
        /// </summary>
        Task<List<Payment>> GetUserPaymentsAsync(Guid userId);

        /// <summary>
        /// L?y danh sách payment c?a loan
        /// </summary>
        Task<List<Payment>> GetLoanPaymentsAsync(int loanId);

        /// <summary>
        /// Ki?m tra payment ?ã thanh toán hay ch?a
        /// </summary>
        Task<bool> IsPaymentSuccessAsync(string orderId);

        /// <summary>
        /// ? NEW: L?y FineAmount c?a Loan
        /// </summary>
        Task<decimal> GetLoanFineAmountAsync(int loanId);

        /// <summary>
        /// ? NEW: Check n?u Loan có ti?n ph?t ch?a thanh toán
        /// </summary>
        Task<bool> HasUnpaidFineAsync(int loanId);

        /// <summary>
        /// ? NEW: Validate Payment Amount vs Loan FineAmount
        /// </summary>
        Task<(bool IsValid, string Message)> ValidatePaymentAmountAsync(int loanId, decimal paymentAmount);
    }
}
