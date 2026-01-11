using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Services.Reports;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILoanReportService _reportService;

        public ReportController(ILoanReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Xu h??ng m??n sách theo period (week/month/year)
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("loan-trend")]
        public async Task<IActionResult> GetLoanTrend(
            [FromQuery] string period = "month",
            [FromQuery] int year = 2026,
            [FromQuery] int? month = null,
            [FromQuery] int? week = null)
        {
            try
            {
                if (string.IsNullOrEmpty(period) || !new[] { "week", "month", "year" }.Contains(period))
                    return BadRequest(new { Error = "Period must be 'week', 'month', or 'year'" });

                var result = await _reportService.GetLoanTrendAsync(period, year, month, week);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Phân b? sách theo th? lo?i
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("category-distribution")]
        public async Task<IActionResult> GetCategoryDistribution(
            [FromQuery] int year = 2026,
            [FromQuery] int month = 1)
        {
            try
            {
                if (month < 1 || month > 12)
                    return BadRequest(new { Error = "Month must be between 1 and 12" });

                var result = await _reportService.GetCategoryDistributionAsync(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Top 5 ng??i dùng tích c?c
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("top-users")]
        public async Task<IActionResult> GetTopUsers(
            [FromQuery] string period = "month",
            [FromQuery] int year = 2026,
            [FromQuery] int? month = null)
        {
            try
            {
                if (string.IsNullOrEmpty(period) || !new[] { "month", "year" }.Contains(period))
                    return BadRequest(new { Error = "Period must be 'month' or 'year'" });

                var result = await _reportService.GetTopUsersAsync(period, year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Top 5 sách hot
        /// </summary>
        [Authorize("AdminOnly")]
        [HttpGet("top-books")]
        public async Task<IActionResult> GetTopBooks(
            [FromQuery] string period = "month",
            [FromQuery] int year = 2026,
            [FromQuery] int? month = null)
        {
            try
            {
                if (string.IsNullOrEmpty(period) || !new[] { "month", "year" }.Contains(period))
                    return BadRequest(new { Error = "Period must be 'month' or 'year'" });

                var result = await _reportService.GetTopBooksAsync(period, year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
