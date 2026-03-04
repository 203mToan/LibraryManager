using MyApi.Libraries;
using MyApi.Model.Request;
using MyApi.Model.Response;
using MyApi.Services.Payments;

namespace MyApi.Services.VnPay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;
        private readonly IPaymentService _paymentService;

        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger, IPaymentService paymentService)
        {
            _configuration = configuration;
            _logger = logger;
            _paymentService = paymentService;
        }
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }
        public async Task<VnPayApiResponse> CreatePaymentUrlAsync(VnPayApiPaymentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OrderId) || request.Amount <= 0)
                {
                    _logger.LogError("CreatePaymentUrlAsync: OrderId hoặc Amount không hợp lệ");
                    return new VnPayApiResponse
                    {
                        Code = -1,
                        Message = "OrderId hoặc Amount không hợp lệ",
                        Success = false
                    };
                }

                var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
                var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
                var expireDate = timeNow.AddMinutes(request.ExpireTime);
                var orderId = request.OrderId;
                if (string.IsNullOrEmpty(orderId) || orderId == "string")
                {
                    orderId = DateTime.Now.Ticks.ToString();
                }
                var existingPayment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                if (existingPayment == null)
                {
                    _logger.LogInformation($"CreatePaymentUrlAsync: No existing payment found, creating new payment record for OrderId={orderId}");
                }

                var pay = new VnPayLibrary();

                pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
                pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
                pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
                pay.AddRequestData("vnp_Amount", (request.Amount * 100).ToString());
                pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
                pay.AddRequestData("vnp_IpAddr", request.IpAddr ?? "127.0.0.1");
                pay.AddRequestData("vnp_Locale", request.Language ?? _configuration["Vnpay:Locale"]);
                pay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
                pay.AddRequestData("vnp_OrderType", request.OrderType ?? "other");
                pay.AddRequestData("vnp_ReturnUrl", _configuration["Vnpay:PaymentBackReturnUrl"]);
                pay.AddRequestData("vnp_TxnRef", orderId);
                pay.AddRequestData("vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss"));

                _logger.LogInformation($"VNPay ReturnUrl: {_configuration["Vnpay:PaymentBackReturnUrl"]}");
                if (!string.IsNullOrEmpty(request.BankCode))
                {
                    pay.AddRequestData("vnp_BankCode", request.BankCode);
                }

                var paymentUrl = pay.CreateRequestUrl(
                    _configuration["Vnpay:BaseUrl"],
                    _configuration["Vnpay:HashSecret"]
                );

                _logger.LogInformation($"CreatePaymentUrlAsync: Created URL for OrderId={orderId}, Amount={request.Amount}");
                _logger.LogInformation($"Full Payment URL: {paymentUrl}");

                return new VnPayApiResponse
                {
                    Code = 0,
                    Message = "Tạo URL thanh toán thành công",
                    Data = paymentUrl,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreatePaymentUrlAsync Error: {ex.Message}");
                return new VnPayApiResponse
                {
                    Code = -1,
                    Message = $"Lỗi: {ex.Message}",
                    Success = false
                };
            }
        }
        public async Task<VnPayApiResponse> VerifyPaymentAsync(IQueryCollection collections)
        {
            try
            {
                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

                _logger.LogInformation($"VerifyPaymentAsync: OrderId={response.OrderId}, ResponseCode={response.VnPayResponseCode}");

                if (!response.Success)
                {
                    _logger.LogError("VerifyPaymentAsync: Xác thực chữ ký thất bại");
                    return new VnPayApiResponse
                    {
                        Code = 97,
                        Message = "Xác thực chữ ký thất bại - Invalid signature",
                        Success = false
                    };
                }
                var responseCode = collections.FirstOrDefault(k => k.Key == "vnp_ResponseCode").Value;
                var transactionStatus = collections.FirstOrDefault(k => k.Key == "vnp_TransactionStatus").Value;

                if (responseCode == "00" && transactionStatus == "00")
                {
                    _logger.LogInformation($"VerifyPaymentAsync: Payment successful for OrderId={response.OrderId}");
                    return new VnPayApiResponse
                    {
                        Code = 0,
                        Message = "Thanh toán thành công",
                        Data = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            response.OrderId,
                            response.TransactionId,
                            response.PaymentId,
                            response.VnPayResponseCode
                        }),
                        Success = true
                    };
                }
                else
                {
                    _logger.LogWarning($"VerifyPaymentAsync: Payment failed with code={responseCode}");
                    return new VnPayApiResponse
                    {
                        Code = int.TryParse(responseCode, out var code) ? code : -1,
                        Message = $"Giao dịch thất bại. Mã lỗi: {responseCode}",
                        Success = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"VerifyPaymentAsync Error: {ex.Message}");
                return new VnPayApiResponse
                {
                    Code = -1,
                    Message = $"Lỗi: {ex.Message}",
                    Success = false
                };
            }
        }
        public async Task<PaymentResponseModel> PaymentExecuteAsync(IQueryCollection collections)
        {
            try
            {
                if (collections == null || collections.Count == 0)
                {
                    _logger.LogWarning("PaymentExecute: Không có dữ liệu callback từ VnPay");
                    return new PaymentResponseModel()
                    {
                        Success = false,
                        VnPayResponseCode = "-1",
                        OrderDescription = "Không có dữ liệu callback từ VnPay"
                    };
                }
                _logger.LogInformation($"PaymentExecute: Received callback with {collections.Count} parameters");
                foreach (var param in collections)
                {
                    _logger.LogInformation($"  {param.Key} = {param.Value}");
                }

                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

                _logger.LogInformation($"PaymentExecute: Processing OrderId={response.OrderId}, Success={response.Success}, ResponseCode={response.VnPayResponseCode}");
                if (!string.IsNullOrEmpty(response.OrderId))
                {
                    await UpdatePaymentStatus(response);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"PaymentExecute Error: {ex.Message}\n{ex.StackTrace}");
                return new PaymentResponseModel()
                {
                    Success = false,
                    VnPayResponseCode = "-1",
                    OrderDescription = $"Lỗi xử lý callback: {ex.Message}"
                };
            }
        }
        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            try
            {
                if (collections == null || collections.Count == 0)
                {
                    _logger.LogWarning("PaymentExecute: Không có dữ liệu callback từ VnPay");
                    return new PaymentResponseModel()
                    {
                        Success = false,
                        VnPayResponseCode = "-1",
                        OrderDescription = "Không có dữ liệu callback từ VnPay"
                    };
                }
                _logger.LogInformation($"PaymentExecute: Received callback with {collections.Count} parameters");
                foreach (var param in collections)
                {
                    _logger.LogInformation($"  {param.Key} = {param.Value}");
                }

                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

                _logger.LogInformation($"PaymentExecute: Processing OrderId={response.OrderId}, Success={response.Success}, ResponseCode={response.VnPayResponseCode}");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"PaymentExecute Error: {ex.Message}\n{ex.StackTrace}");
                return new PaymentResponseModel()
                {
                    Success = false,
                    VnPayResponseCode = "-1",
                    OrderDescription = $"Lỗi xử lý callback: {ex.Message}"
                };
            }
        }
        private async Task UpdatePaymentStatus(PaymentResponseModel response)
        {
            try
            {
                var existingPayment = await _paymentService.GetPaymentByOrderIdAsync(response.OrderId);
                if (existingPayment == null)
                {
                    _logger.LogWarning($"UpdatePaymentStatus: Payment not found OrderId={response.OrderId}. Skipping update (test mode or direct API call).");
                    return; 
                }

                if (response.Success && response.VnPayResponseCode == "00")
                {
                    await _paymentService.UpdatePaymentSuccessAsync(
                        response.OrderId,
                        response.TransactionId ?? "",
                        response.VnPayResponseCode
                    );
                    _logger.LogInformation($"UpdatePaymentStatus: Payment Success - OrderId={response.OrderId}");
                }
                else
                {
                    var errorMsg = $"VNPay Error Code: {response.VnPayResponseCode}";
                    await _paymentService.UpdatePaymentFailedAsync(response.OrderId, errorMsg);
                    _logger.LogWarning($"UpdatePaymentStatus: Payment Failed - OrderId={response.OrderId}, Error={errorMsg}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdatePaymentStatus Error: {ex.Message}");
            }
        }
    }
}
