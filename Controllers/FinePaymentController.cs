using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.FinePayments;
using MyApi.Services.Payments;
using MyApi.Services.VnPay;
using System.Security.Claims;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinePaymentController : ControllerBase
    {
        private readonly IFinePaymentService _finePaymentService;
        private readonly IPaymentService _paymentService;
        private readonly IVnPayService _vnPayService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FinePaymentController> _logger;

        public FinePaymentController(
            IFinePaymentService finePaymentService,
            IPaymentService paymentService,
            IVnPayService vnPayService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FinePaymentController> logger)
        {
            _finePaymentService = finePaymentService;
            _paymentService = paymentService;
            _vnPayService = vnPayService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ============================================
        // ADMIN ENDPOINTS (Existing)
        // ============================================

        /// <summary>
        /// L?y t?t c? các kho?n thanh toán ph?t
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllFinePayments()
        {
            try
            {
                var result = await _finePaymentService.GetAllFinePaymentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// L?y báo cáo thanh toán ph?t theo kho?ng ngày
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("by-date")]
        public async Task<IActionResult> GetFinePaymentsByDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _finePaymentService.GetFinePaymentsByDateAsync(startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// L?y báo cáo thanh toán ph?t theo tu?n
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("by-week")]
        public async Task<IActionResult> GetFinePaymentsByWeek([FromQuery] int week, [FromQuery] int year)
        {
            try
            {
                if (week < 1 || week > 53)
                    return BadRequest(new { Error = "Week must be between 1 and 53" });
                
                var result = await _finePaymentService.GetFinePaymentsByWeekAsync(week, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// L?y báo cáo thanh toán ph?t theo tháng
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("by-month")]
        public async Task<IActionResult> GetFinePaymentsByMonth([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest(new { Error = "Month must be between 1 and 12" });
                
                var result = await _finePaymentService.GetFinePaymentsByMonthAsync(month, year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// L?y báo cáo thanh toán ph?t theo n?m
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("by-year")]
        public async Task<IActionResult> GetFinePaymentsByYear([FromQuery] int year)
        {
            try
            {
                var result = await _finePaymentService.GetFinePaymentsByYearAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// L?y th?ng kê t?ng doanh thu ph?t (ngày, tu?n, tháng, n?m)
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetFinePaymentStatistics()
        {
            try
            {
                var result = await _finePaymentService.GetFinePaymentStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // ============================================
        // USER ENDPOINTS (NEW - VNPay Payment)
        // ============================================

        /// <summary>
        /// Check Fine Amount c?a Loan
        /// GET /api/finepayment/check-fine/{loanId}
        /// </summary>
        [Authorize]
        [HttpGet("check-fine/{loanId}")]
        public async Task<IActionResult> CheckFineAmount(int loanId)
        {
            try
            {
                var fineAmount = await _paymentService.GetLoanFineAmountAsync(loanId);
                var hasUnpaidFine = await _paymentService.HasUnpaidFineAsync(loanId);

                return Ok(new
                {
                    success = true,
                    loanId,
                    fineAmount,
                    hasUnpaidFine,
                    message = hasUnpaidFine 
                        ? $"Có ti?n ph?t c?n thanh toán: {fineAmount:N0} VND" 
                        : "Không có ti?n ph?t"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"CheckFineAmount Error: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = $"L?i: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Kh?i t?o Payment cho Ti?n Ph?t VNPay
        /// POST /api/finepayment/initiate-vnpay-payment
        /// </summary>
        [Authorize]
        [HttpPost("initiate-vnpay-payment")]
        public async Task<IActionResult> InitiateVNPayPayment([FromBody] InitiateFinePaymentRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                // 1. ? Validate Loan và Fine Amount
                var (isValid, validationMessage) = await _paymentService.ValidatePaymentAmountAsync(
                    request.LoanId,
                    request.Amount
                );

                if (!isValid)
                {
                    _logger.LogWarning($"InitiateVNPayPayment: Validation failed - {validationMessage}");
                    return BadRequest(new
                    {
                        success = false,
                        message = validationMessage
                    });
                }

                // 2. ? T?o Payment record
                var payment = await _paymentService.CreatePaymentAsync(
                    userId: userGuid,
                    loanId: request.LoanId,
                    amount: request.Amount,
                    orderInfo: $"Thanh toán ti?n ph?t - M??n sách #{request.LoanId}",
                    expiredAt: DateTime.UtcNow.AddMinutes(15)
                );

                // 3. ? T?o VNPay URL
                var vnPayRequest = new VnPayApiPaymentRequest
                {
                    OrderId = payment.OrderId,
                    Amount = (long)request.Amount,
                    OrderInfo = $"Thanh toán ti?n ph?t tr? h?n - M??n sách #{request.LoanId}",
                    Language = request.Language ?? "vn",
                    BankCode = request.BankCode,
                    ExpireTime = 15
                };

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                vnPayRequest.IpAddr = ipAddress;

                var vnPayResponse = await _vnPayService.CreatePaymentUrlAsync(vnPayRequest);

                if (!vnPayResponse.Success)
                {
                    _logger.LogError($"InitiateVNPayPayment: VNPay create URL failed - {vnPayResponse.Message}");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"L?i t?o URL thanh toán: {vnPayResponse.Message}"
                    });
                }

                _logger.LogInformation(
                    $"InitiateVNPayPayment: Payment initiated - " +
                    $"OrderId={payment.OrderId}, UserId={userGuid}, LoanId={request.LoanId}, " +
                    $"Amount={request.Amount}");

                return Ok(new
                {
                    success = true,
                    message = "Kh?i t?o thanh toán thành công",
                    data = new
                    {
                        orderId = payment.OrderId,
                        amount = request.Amount,
                        paymentUrl = vnPayResponse.Data,
                        expiresIn = 15 // minutes
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"InitiateVNPayPayment Error: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = $"L?i: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get Payment Status
        /// GET /api/finepayment/payment-status/{orderId}
        /// </summary>
        [Authorize]
        [HttpGet("payment-status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(string orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                
                if (payment == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Payment not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId = payment.OrderId,
                        amount = payment.Amount,
                        status = payment.Status,
                        createdAt = payment.CreatedAt,
                        paidAt = payment.PaidAt,
                        transactionId = payment.TransactionId,
                        vnPayResponseCode = payment.VnPayResponseCode,
                        errorMessage = payment.ErrorMessage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPaymentStatus Error: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = $"L?i: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get User Fine Payments History
        /// GET /api/finepayment/my-payments
        /// </summary>
        [Authorize]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    return Unauthorized(new { success = false, message = "User not found" });
                }

                var payments = await _paymentService.GetUserPaymentsAsync(userGuid);

                return Ok(new
                {
                    success = true,
                    data = payments.Select(p => new
                    {
                        id = p.Id,
                        orderId = p.OrderId,
                        amount = p.Amount,
                        status = p.Status,
                        loanId = p.LoanId,
                        createdAt = p.CreatedAt,
                        paidAt = p.PaidAt,
                        transactionId = p.TransactionId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMyPayments Error: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = $"L?i: {ex.Message}"
                });
            }
        }
    }
}
