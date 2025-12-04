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
            var result = await _loanService.CreateLoanAsync(request);
            return Ok(result);
        }

        [Authorize("AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var result = await _loanService.GetAllLoansAsync();
            return Ok(result);
        }

        // GET: api/loan/{id}
        [Authorize("AdminOrUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            var result = await _loanService.GetLoanByIdAsync(id);

            if (result == null) return NotFound();

            return Ok(result);
        }
        [Authorize("AdminOrUser")]
        [HttpPut("return/{id}")]
        public async Task<IActionResult> ReturnBook(int id, [FromBody] DateTime returnDate)
        {
            var result = await _loanService.ReturnBookAsync(id, returnDate);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyLoans()
        {
            var result = await _loanService.GetMyLoansAsync();
            return Ok(result);
        }

        [Authorize("AdminOnly")]
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveLoan(int id)
        {
            var result = await _loanService.ApproveLoanAsync(id);

            if (result == null)
                return NotFound(new { Message = "Loan not found" });

            return Ok(result);
        }
        [HttpPut("pay-fine/{id}")]
        public async Task<IActionResult> PayFine(int id)
        {
            try
            {
                var result = await _loanService.PayFineAsync(id);

                if (result == null)
                    return NotFound(new { Message = "Loan not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
