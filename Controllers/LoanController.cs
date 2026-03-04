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
        [Authorize("AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans(
            [FromQuery] int? pageIndex = 1, 
            [FromQuery] int? pageSize = 10,
            [FromQuery] string? status = null)
        {
            var result = await _loanService.GetAllLoansPagedAsync(pageIndex, pageSize, status);
            return Ok(result);
        }
        [Authorize("AdminOrUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            var result = await _loanService.GetLoanByIdAsync(id);

            if (result == null) 
                return NotFound(new { Message = "Loan not found" });

            return Ok(result);
        }
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
        [Authorize("AdminOrUser")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLoans(
            [FromQuery] int? pageIndex = 1, 
            [FromQuery] int? pageSize = 10,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _loanService.GetMyLoansPagedAsync(pageIndex, pageSize, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
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
        [Authorize("AdminOnly")]
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
                    Message = "Return approved successfully. Book returned to stock.",
                    Status = result.Status,
                    Loan = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
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
