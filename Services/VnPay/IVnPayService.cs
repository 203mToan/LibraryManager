using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.VnPay
{
    public interface IVnPayService
    {
        /// <summary>
        /// Create payment URL for redirect method (Traditional)
        /// </summary>
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);

        /// <summary>
        /// Create payment URL using API method (Dev Environment)
        /// </summary>
        Task<VnPayApiResponse> CreatePaymentUrlAsync(VnPayApiPaymentRequest request);

        /// <summary>
        /// Verify payment from VnPay callback
        /// </summary>
        Task<VnPayApiResponse> VerifyPaymentAsync(IQueryCollection collections);

        /// <summary>
        /// Get payment status (Return URL callback) - Async version with DB update
        /// </summary>
        Task<PaymentResponseModel> PaymentExecuteAsync(IQueryCollection collections);

        /// <summary>
        /// Get payment status (Return URL callback) - Sync version (backward compat)
        /// </summary>
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
