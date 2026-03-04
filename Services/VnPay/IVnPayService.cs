using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.VnPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        Task<VnPayApiResponse> CreatePaymentUrlAsync(VnPayApiPaymentRequest request);
        Task<VnPayApiResponse> VerifyPaymentAsync(IQueryCollection collections);
        Task<PaymentResponseModel> PaymentExecuteAsync(IQueryCollection collections);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
