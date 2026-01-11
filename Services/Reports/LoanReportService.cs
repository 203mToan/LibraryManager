using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Response;
using System.Globalization;

namespace MyApi.Services.Reports
{
    public class LoanReportService : ILoanReportService
    {
        private readonly AppDbContext _db;

        public LoanReportService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<LoanTrendResponse> GetLoanTrendAsync(string period, int year, int? month = null, int? week = null)
        {
            // ? Load t?t c? loans vào memory tr??c, sau ?ó x? lý datetime
            var loans = await _db.Loans
                .Where(x => x.LoanDate.HasValue)
                .ToListAsync();

            var trendData = new List<TrendDataPoint>();

            if (period == "week" && week.HasValue)
            {
                var startDate = GetDateFromWeek(year, week.Value);
                for (int day = 0; day < 7; day++)
                {
                    var date = startDate.AddDays(day);
                    var label = $"Tu?n {date.Day}";

                    // ? Filter trong memory, không trong query
                    var loanCount = loans.Count(x => x.LoanDate.Value.ToUniversalTime().Date == date.Date);
                    var returnCount = loans.Count(x => x.ReturnDate.HasValue && x.ReturnDate.Value.ToUniversalTime().Date == date.Date);

                    trendData.Add(new TrendDataPoint
                    {
                        Label = label,
                        LoanCount = loanCount,
                        ReturnCount = returnCount
                    });
                }
            }
            else if (period == "month" && month.HasValue)
            {
                var daysInMonth = DateTime.DaysInMonth(year, month.Value);
                var startDate = new DateTime(year, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);

                for (int week_num = 1; week_num <= 5; week_num++)
                {
                    var weekStart = startDate.AddDays((week_num - 1) * 7);
                    var weekEnd = weekStart.AddDays(6);

                    if (weekStart.Month != month.Value) break;

                    // ? Filter trong memory
                    var loanCount = loans.Count(x => x.LoanDate.HasValue &&
                        x.LoanDate.Value.ToUniversalTime().Date >= weekStart.Date &&
                        x.LoanDate.Value.ToUniversalTime().Date <= weekEnd.Date);
                    var returnCount = loans.Count(x => x.ReturnDate.HasValue &&
                        x.ReturnDate.Value.ToUniversalTime().Date >= weekStart.Date &&
                        x.ReturnDate.Value.ToUniversalTime().Date <= weekEnd.Date);

                    trendData.Add(new TrendDataPoint
                    {
                        Label = $"Tu?n {week_num}",
                        LoanCount = loanCount,
                        ReturnCount = returnCount
                    });
                }
            }
            else if (period == "year")
            {
                for (int m = 1; m <= 12; m++)
                {
                    var startDate = new DateTime(year, m, 1, 0, 0, 0, DateTimeKind.Utc);
                    var endDate = startDate.AddMonths(1).AddDays(-1);

                    // ? Filter trong memory
                    var loanCount = loans.Count(x => x.LoanDate.HasValue &&
                        x.LoanDate.Value.ToUniversalTime().Date >= startDate.Date &&
                        x.LoanDate.Value.ToUniversalTime().Date <= endDate.Date);
                    var returnCount = loans.Count(x => x.ReturnDate.HasValue &&
                        x.ReturnDate.Value.ToUniversalTime().Date >= startDate.Date &&
                        x.ReturnDate.Value.ToUniversalTime().Date <= endDate.Date);

                    trendData.Add(new TrendDataPoint
                    {
                        Label = $"Tháng {m}",
                        LoanCount = loanCount,
                        ReturnCount = returnCount
                    });
                }
            }

            return new LoanTrendResponse
            {
                Period = period,
                Data = trendData
            };
        }

        public async Task<CategoryDistributionResponse> GetCategoryDistributionAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // ? Load t?t c? loans vào memory tr??c
            var loans = await _db.Loans
                .Include(x => x.Book)
                .ThenInclude(x => x.Category)
                .ToListAsync();

            // ? Filter trong memory
            var filteredLoans = loans
                .Where(x => x.LoanDate.HasValue &&
                           x.LoanDate.Value.ToUniversalTime().Date >= startDate.Date &&
                           x.LoanDate.Value.ToUniversalTime().Date <= endDate.Date)
                .ToList();

            var categoryGroups = filteredLoans
                .GroupBy(x => new { x.Book.CategoryId, x.Book.Category.Name })
                .Select(g => new CategoryDistributionData
                {
                    CategoryId = g.Key.CategoryId ?? 0,
                    CategoryName = g.Key.Name ?? "Unknown",
                    LoanCount = g.Count()
                })
                .OrderByDescending(x => x.LoanCount)
                .ToList();

            var totalLoans = filteredLoans.Count;

            foreach (var category in categoryGroups)
            {
                category.Percentage = totalLoans > 0 ? Math.Round((decimal)category.LoanCount / totalLoans * 100, 2) : 0;
            }

            return new CategoryDistributionResponse
            {
                Categories = categoryGroups,
                TotalLoans = totalLoans
            };
        }

        public async Task<TopUserResponse> GetTopUsersAsync(string period, int year, int? month = null)
        {
            DateTime startDate, endDate;

            if (period == "month" && month.HasValue)
            {
                startDate = new DateTime(year, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (period == "year")
            {
                startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            }
            else
            {
                startDate = DateTime.UtcNow.Date;
                endDate = startDate;
            }

            // ? Load t?t c? loans vào memory
            var loans = await _db.Loans
                .Include(x => x.User)
                .ToListAsync();

            // ? Filter trong memory
            var filteredLoans = loans
                .Where(x => x.LoanDate.HasValue &&
                           x.LoanDate.Value.ToUniversalTime().Date >= startDate.Date &&
                           x.LoanDate.Value.ToUniversalTime().Date <= endDate.Date)
                .ToList();

            var userStats = filteredLoans
                .GroupBy(x => x.UserId)
                .Select(g => new UserStatistics
                {
                    UserId = g.Key,
                    UserName = g.First().User?.FullName ?? "Unknown",
                    LoanCount = g.Count(),
                    ReturnedCount = g.Count(x => x.Status == LoanStatus.Returned.ToString() || x.Status == LoanStatus.Paid.ToString()),
                    OverdueCount = g.Count(x => x.Status == LoanStatus.Overdue.ToString())
                })
                .OrderByDescending(x => x.LoanCount)
                .Take(5)
                .ToList();

            return new TopUserResponse
            {
                Users = userStats
            };
        }

        public async Task<TopBookResponse> GetTopBooksAsync(string period, int year, int? month = null)
        {
            DateTime startDate, endDate;

            if (period == "month" && month.HasValue)
            {
                startDate = new DateTime(year, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else if (period == "year")
            {
                startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            }
            else
            {
                startDate = DateTime.UtcNow.Date;
                endDate = startDate;
            }

            // ? Load t?t c? loans vào memory
            var loans = await _db.Loans
                .Include(x => x.Book)
                .ThenInclude(x => x.Category)
                .ToListAsync();

            // ? Filter trong memory
            var filteredLoans = loans
                .Where(x => x.LoanDate.HasValue &&
                           x.LoanDate.Value.ToUniversalTime().Date >= startDate.Date &&
                           x.LoanDate.Value.ToUniversalTime().Date <= endDate.Date)
                .ToList();

            var bookStats = filteredLoans
                .GroupBy(x => x.BookId)
                .Select(g => new BookStatistics
                {
                    BookId = g.Key,
                    BookTitle = g.First().Book?.Title ?? "Unknown",
                    CategoryName = g.First().Book?.Category?.Name ?? "Unknown",
                    LoanCount = g.Count(),
                    ReturnedCount = g.Count(x => x.Status == LoanStatus.Returned.ToString() || x.Status == LoanStatus.Paid.ToString())
                })
                .OrderByDescending(x => x.LoanCount)
                .Take(5)
                .ToList();

            return new TopBookResponse
            {
                Books = bookStats
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
