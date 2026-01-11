using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.Loans;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>
        /// Tạo yêu cầu mượn sách mới
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanRequest request)
        {
            try
            {
                var result = await _loanService.CreateLoanAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả các khoản mượn (Admin only)
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var result = await _loanService.GetAllLoansAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một khoản mượn
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            var result = await _loanService.GetLoanByIdAsync(id);

            if (result == null) 
                return NotFound(new { Message = "Loan not found" });

            return Ok(result);
        }

        /// <summary>
        /// Trả sách
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id, [FromBody] DateTime returnDate)
        {
            try
            {
                var result = await _loanService.ReturnBookAsync(id, returnDate);

                if (result == null) 
                    return NotFound(new { Message = "Loan not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách các khoản mượn của người dùng hiện tại
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLoans()
        {
            try
            {
                var result = await _loanService.GetMyLoansAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Phê duyệt yêu cầu mượn sách (Admin only)
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveLoan(int id)
        {
            try
            {
                var result = await _loanService.ApproveLoanAsync(id);

                if (result == null)
                    return NotFound(new { Message = "Loan not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
       
        /// <summary>
        /// Hủy yêu cầu mượn sách
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelLoan(int id)
        {
            try
            {
                var result = await _loanService.CancelLoanAsync(id);

                if (result == null)
                    return NotFound(new { Message = "Loan not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// User thanh toán tiền phạt - chờ admin duyệt
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpPut("send-request-pay-fine/{id}")]
        public async Task<IActionResult> PayFine(int id)
        {
            try
            {
                var result = await _loanService.PayFineAsync(id);

                if (result == null)
                    return NotFound(new { Message = "Loan not found" });

                return Ok(new 
                { 
                    Message = "Payment request sent successfully. Waiting for admin approval.",
                    Status = result.Status,
                    FineAmount = result.FineAmount,
                    Loan = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }   

        /// <summary>
        /// Admin duyệt thanh toán tiền phạt
        /// </summary>
        [Authorize("AdminOrUser")]
        [HttpPut("approve-payment/{id}")]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            try
            {
                var result = await _loanService.ApprovePaymentAsync(id);

                if (result == null)
                    return NotFound(new { Message = "Loan not found" });

                return Ok(new 
                { 
                    Message = "Payment approved successfully. Loan returned.",
                    Status = result.Status,
                    FineAmount = result.FineAmount,
                    Loan = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ Lấy tóm tắt thống kê mượn sách theo tháng/năm
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("summary/by-period")]
        public async Task<IActionResult> GetLoanSummaryByPeriod([FromQuery] int year, [FromQuery] int? month = null)
        {
            try
            {
                if (month.HasValue && (month < 1 || month > 12))
                    return BadRequest(new { Error = "Month must be between 1 and 12" });

                var result = await _loanService.GetLoanSummaryByPeriodAsync(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
