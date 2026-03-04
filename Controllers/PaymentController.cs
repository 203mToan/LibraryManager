using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Model.Response;
using MyApi.Services.VnPay;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IVnPayService vnPayService, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<PaymentController> logger,
            IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }
        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.OrderType) || model.Amount <= 0)
                return BadRequest(new { message = "Dữ liệu thanh toán không hợp lệ" });

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { paymentUrl = url });
        }
        [HttpPost("create-payment-url-api")]
        public async Task<IActionResult> CreatePaymentUrlApiAsync([FromBody] VnPayApiPaymentRequest request)
        {
            if (string.IsNullOrEmpty(request?.OrderId) || request.Amount <= 0)
                return BadRequest(new VnPayApiResponse
                {
                    Code = -1,
                    Message = "OrderId hoặc Amount không hợp lệ",
                    Success = false
                });
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            request.IpAddr = ipAddress;

            _logger.LogInformation($"CreatePaymentUrlApi: OrderId={request.OrderId}, Amount={request.Amount}");

            var response = await _vnPayService.CreatePaymentUrlAsync(request);
            return Ok(response);
        }
        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                Console.WriteLine("=== PAYMENT CALLBACK TRIGGERED ===");
                Console.WriteLine($"QueryString: {Request.QueryString}");
                
                _logger.LogInformation($"PaymentCallback: Received request from {Request.QueryString}");
                foreach (var param in Request.Query)
                {
                    Console.WriteLine($"  {param.Key} = {param.Value}");
                    _logger.LogInformation($"  {param.Key} = {param.Value}");
                }
                var response = await _vnPayService.PaymentExecuteAsync(Request.Query);
                
                Console.WriteLine($"Response received: Success={response?.Success}, Code={response?.VnPayResponseCode}");
                var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();
                var vnpTxnRef = Request.Query["vnp_TxnRef"].ToString();
                var vnpTransactionNo = Request.Query["vnp_TransactionNo"].ToString();
                var vnpAmount = Request.Query["vnp_Amount"].ToString();
                var isSuccess = response?.Success == true && vnpResponseCode == "00";
                var orderId = !string.IsNullOrEmpty(response?.OrderId) ? response.OrderId : vnpTxnRef;
                var amountDisplay = "0";
                if (long.TryParse(vnpAmount, out var amountValue))
                {
                    amountDisplay = (amountValue / 100).ToString("N0");
                }
                var frontendUrl = _configuration["Frontend:Url"];
                
                if (!string.IsNullOrEmpty(frontendUrl))
                {
                    var queryParams = $"payment={(isSuccess ? "success" : "failed")}&orderId={Uri.EscapeDataString(orderId)}&transactionNo={Uri.EscapeDataString(vnpTransactionNo)}&amount={Uri.EscapeDataString(amountDisplay)}";
                    var myLoansUrl = $"{frontendUrl}/myloans?{queryParams}";
                    _logger.LogInformation($"PaymentCallback: Redirecting to myloans: {myLoansUrl}");
                    return Redirect(myLoansUrl);
                }
                return Redirect($"/payment-result.html?success={isSuccess.ToString().ToLower()}&orderId={Uri.EscapeDataString(orderId)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in PaymentCallback: {ex.Message}");
                _logger.LogError($"PaymentCallback Exception: {ex.Message}\n{ex.StackTrace}");
                
                var frontendUrl = _configuration["Frontend:Url"];
                if (!string.IsNullOrEmpty(frontendUrl))
                {
                    return Redirect($"{frontendUrl}/myloans?payment=failed&error={Uri.EscapeDataString(ex.Message)}");
                }
                
                return Redirect($"/payment-result.html?success=false&error={Uri.EscapeDataString(ex.Message)}");
            }
        }
        private string GetVnPayErrorMessage(string code)
        {
            var messages = new Dictionary<string, string>
            {
                { "00", "Giao dịch thành công" },
                { "07", "Trừ tiền thành công nhưng giao dịch bị nghi ngờ" },
                { "09", "Thẻ/Tài khoản chưa đăng ký InternetBanking" },
                { "10", "Xác thực thông tin thẻ không đúng quá 3 lần" },
                { "11", "Đã hết hạn chờ thanh toán" },
                { "12", "Thẻ/Tài khoản bị khóa" },
                { "13", "Sai mật khẩu OTP" },
                { "24", "Khách hàng hủy giao dịch" },
                { "51", "Tài khoản không đủ số dư" },
                { "65", "Vượt quá hạn mức giao dịch trong ngày" },
                { "75", "Ngân hàng đang bảo trì" },
                { "79", "Nhập sai mật khẩu quá số lần quy định" },
                { "97", "Chữ ký không hợp lệ" },
                { "99", "Lỗi không xác định" }
            };
            
            return messages.TryGetValue(code ?? "", out var message) ? message : "Giao dịch thất bại";
        }
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment()
        {
            _logger.LogInformation("VerifyPayment: Request received");
            var response = await _vnPayService.VerifyPaymentAsync(Request.Query);
            return Ok(response);
        }
        [HttpGet("payment-callback-vnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            _logger.LogInformation($"PaymentCallbackVnpay: Received request from {Request.QueryString}");
            var response = await _vnPayService.PaymentExecuteAsync(Request.Query);
            return Ok(response);
        }
        [HttpGet("payment-ipn")]
        [HttpPost("payment-ipn")]
        public async Task<IActionResult> PaymentIPN()
        {
            try
            {
                _logger.LogInformation($"PaymentIPN: Received callback from VNPay {Request.QueryString}");

                var response = await _vnPayService.PaymentExecuteAsync(Request.Query);

                if (response == null)
                {
                    _logger.LogError("PaymentIPN: Response is null");
                    return Ok(new { RspCode = "99", Message = "Unknown error" });
                }

                // Trả về JSON response theo format VNPay yêu cầu
                if (response.Success && response.VnPayResponseCode == "00")
                {
                    _logger.LogInformation($"PaymentIPN: Payment success - OrderId={response.OrderId}");
                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    _logger.LogWarning($"PaymentIPN: Payment failed - Code={response.VnPayResponseCode}");
                    return Ok(new { RspCode = response.VnPayResponseCode, Message = "Confirm Fail" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"PaymentIPN Exception: {ex.Message}\n{ex.StackTrace}");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }
    }
}
