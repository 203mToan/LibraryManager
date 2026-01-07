using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Response;
using System.Globalization;

namespace MyApi.Services.FinePayments
{
    public class FinePaymentService : IFinePaymentService
    {
        private readonly AppDbContext _db;

        public FinePaymentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<FinePaymentResponse> CreateFinePaymentAsync(Guid userId, int loanId, int amount, string? paymentMethod = null, string? description = null)
        {
            var finePayment = new FinePayment
            {
                UserId = userId,
                LoanId = loanId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod ?? "Cash",
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.FinePayments.AddAsync(finePayment);
            await _db.SaveChangesAsync();

            var loan = await _db.Loans.Include(x => x.Book).FirstOrDefaultAsync(x => x.Id == loanId);
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);

            return new FinePaymentResponse
            {
                Id = finePayment.Id,
                UserId = finePayment.UserId,
                UserName = user?.FullName,
                LoanId = finePayment.LoanId,
                BookName = loan?.Book?.Title,
                Amount = finePayment.Amount,
                PaymentDate = finePayment.PaymentDate,
                PaymentMethod = finePayment.PaymentMethod,
                Description = finePayment.Description
            };
        }

        public async Task<FinePaymentReportResponse> GetAllFinePaymentsAsync()
        {
            var payments = await _db.FinePayments
                .Include(x => x.User)
                .Include(x => x.Loan)
                .ThenInclude(x => x.Book)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            var response = new FinePaymentReportResponse
            {
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(x => x.Amount),
                Payments = payments.Select(x => new FinePaymentResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    LoanId = x.LoanId,
                    BookName = x.Loan?.Book?.Title,
                    Amount = x.Amount,
                    PaymentDate = x.PaymentDate,
                    PaymentMethod = x.PaymentMethod,
                    Description = x.Description
                }).ToList()
            };

            return response;
        }

        public async Task<FinePaymentReportResponse> GetFinePaymentsByDateAsync(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddDays(1).AddSeconds(-1);

            var payments = await _db.FinePayments
                .Include(x => x.User)
                .Include(x => x.Loan)
                .ThenInclude(x => x.Book)
                .Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            var response = new FinePaymentReportResponse
            {
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(x => x.Amount),
                Payments = payments.Select(x => new FinePaymentResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    LoanId = x.LoanId,
                    BookName = x.Loan?.Book?.Title,
                    Amount = x.Amount,
                    PaymentDate = x.PaymentDate,
                    PaymentMethod = x.PaymentMethod,
                    Description = x.Description
                }).ToList()
            };

            return response;
        }

        public async Task<FinePaymentReportResponse> GetFinePaymentsByWeekAsync(int week, int year)
        {
            var startDate = GetDateFromWeek(year, week);
            var endDate = startDate.AddDays(6);
            return await GetFinePaymentsByDateAsync(startDate, endDate);
        }

        public async Task<FinePaymentReportResponse> GetFinePaymentsByMonthAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return await GetFinePaymentsByDateAsync(startDate, endDate);
        }

        public async Task<FinePaymentReportResponse> GetFinePaymentsByYearAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            return await GetFinePaymentsByDateAsync(startDate, endDate);
        }

        public async Task<FinePaymentReportResponse> GetUserFinePaymentsAsync(Guid userId)
        {
            var payments = await _db.FinePayments
                .Include(x => x.User)
                .Include(x => x.Loan)
                .ThenInclude(x => x.Book)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            var response = new FinePaymentReportResponse
            {
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(x => x.Amount),
                Payments = payments.Select(x => new FinePaymentResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.FullName,
                    LoanId = x.LoanId,
                    BookName = x.Loan?.Book?.Title,
                    Amount = x.Amount,
                    PaymentDate = x.PaymentDate,
                    PaymentMethod = x.PaymentMethod,
                    Description = x.Description
                }).ToList()
            };

            return response;
        }

        public async Task<FinePaymentStatisticsResponse> GetFinePaymentStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfDay = now.Date;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var dailyTotal = await _db.FinePayments
                .Where(x => x.PaymentDate >= startOfDay)
                .SumAsync(x => x.Amount);

            var weeklyTotal = await _db.FinePayments
                .Where(x => x.PaymentDate >= startOfWeek)
                .SumAsync(x => x.Amount);

            var monthlyTotal = await _db.FinePayments
                .Where(x => x.PaymentDate >= startOfMonth)
                .SumAsync(x => x.Amount);

            var yearlyTotal = await _db.FinePayments
                .Where(x => x.PaymentDate >= startOfYear)
                .SumAsync(x => x.Amount);

            var byDate = await _db.FinePayments
                .Where(x => x.PaymentDate >= startOfMonth)
                .GroupBy(x => x.PaymentDate.Date)
                .Select(g => new FinePaymentByDateResponse
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Total = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new FinePaymentStatisticsResponse
            {
                DailyTotal = dailyTotal,
                WeeklyTotal = weeklyTotal,
                MonthlyTotal = monthlyTotal,
                YearlyTotal = yearlyTotal,
                ByDate = byDate
            };
        }

        private DateTime GetDateFromWeek(int year, int week)
        {
            var jan4 = new DateTime(year, 1, 4);
            var daysOffset = (int)jan4.DayOfWeek - (int)DayOfWeek.Monday;
            var firstMonday = jan4.AddDays(-daysOffset);
            var weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan4, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var firstWeekOfYear = firstMonday.AddDays((week - weekNumber) * 7);
            return firstWeekOfYear;
        }
    }
}
