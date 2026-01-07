using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Services.FinePayments;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinePaymentController : ControllerBase
    {
        private readonly IFinePaymentService _finePaymentService;

        public FinePaymentController(IFinePaymentService finePaymentService)
        {
            _finePaymentService = finePaymentService;
        }

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
    }
}
