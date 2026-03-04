using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using MyApi.Services.Identity;
using MyApi.Utils;
using System.Security.Principal;

namespace MyApi.Services.Loans
{
    public class LoanService : ILoanService
    {
        private readonly AppDbContext _db;
        private readonly IIdentityService _identity;

        public LoanService(AppDbContext db, IIdentityService identityService)
        {
            _db = db;
            _identity = identityService;
        }

        public async Task<LoanResponse> CreateLoanAsync(LoanRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user == null)
                throw new Exception("User not found");
            var userLoans = await _db.Loans
                .Where(x => x.UserId == request.UserId)
                .ToListAsync();

            var hasOverdue = userLoans.Any(x =>
                x.Status == LoanStatus.Overdue.ToString() ||
                (x.DueDate.HasValue && DateTime.UtcNow > x.DueDate.Value && x.Status == LoanStatus.Approved.ToString())
            );

            if (hasOverdue)
            {
                throw new Exception("User has overdue loans");
            }

            var book = await _db.Books.FirstOrDefaultAsync(x => x.Id == request.BookId);
            if (book == null)
                throw new Exception("Book not found");

            if (book.StockQuantity <= 0)
                throw new Exception("Book is not available");

            var loan = request.ToEntity();
            loan.Status = LoanStatus.Pending.ToString();

            await _db.Loans.AddAsync(loan);
            book.StockQuantity -= 1;

            await _db.SaveChangesAsync();

            return new LoanResponse().ToResponse(loan);
        }

        public async Task<List<LoanResponse>> GetAllLoansAsync()
        {
            var loans = await _db.Loans
                .Include(x => x.User)
                .Include(x => x.Book)
                .OrderByDescending(x => x.LoanDate)
                .ToListAsync();
            await UpdateOverdueStatusAsync(loans);
            return loans.Select(x => new LoanResponse().ToResponse(x)).ToList();
        }

        public async Task<PagedHttpResponse<LoanResponse>> GetAllLoansPagedAsync(int? pageIndex, int? pageSize, string? status = null)
        {
            var query = _db.Loans
                .Include(x => x.User)
                .Include(x => x.Book)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }
            var totalItems = await query.CountAsync();
            var allLoans = await query
                .OrderByDescending(x => x.LoanDate)
                .ToListAsync();
            
            await UpdateOverdueStatusAsync(allLoans);
            IEnumerable<Loan> pagedLoans = allLoans;
            if (pageIndex.HasValue && pageSize.HasValue && pageIndex > 0 && pageSize > 0)
            {
                pagedLoans = allLoans
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var loanResponses = pagedLoans.Select(x => new LoanResponse().ToResponse(x));

            return new PagedHttpResponse<LoanResponse>
            {
                TotalItems = totalItems,
                TotalPages = PaginationUtils.TotalPagesConversion(totalItems, pageSize),
                Items = loanResponses
            };
        }

        public async Task<LoanResponse?> GetLoanByIdAsync(int id)
        {
            var loan = await _db.Loans
                .Include(x => x.User)
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);
                
            if (loan == null) return null;
            
            var loans = new List<Loan> { loan };
            await UpdateOverdueStatusAsync(loans);

            return new LoanResponse().ToResponse(loan);
        }

        public async Task<LoanResponse?> ReturnBookAsync(int id, DateTime returnDate)
        {
            var loan = await _db.Loans
                .Include(x => x.User)
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (loan == null) return null;

            if (loan.Status == LoanStatus.Returned.ToString() || loan.Status == LoanStatus.Paid.ToString())
                throw new Exception("Book already returned");

            loan.ReturnDate = returnDate;
            if (loan.DueDate.HasValue && returnDate > loan.DueDate.Value)
            {
                // ✅ Tính tiền phạt: số ngày trễ × 20000đ
                int overdueDays = (int)(returnDate - loan.DueDate.Value).TotalDays;
                loan.FineAmount = overdueDays * 20000;
                loan.Status = LoanStatus.Overdue.ToString(); // Đánh dấu quá hạn, chờ thanh toán
            }
            else
            {
                loan.Status = LoanStatus.Returned.ToString(); // Trả đúng hạn
                loan.FineAmount = 0; // Không có phạt
            }
            
            loan.UpdatedAt = DateTime.UtcNow;
            loan.Book.StockQuantity += 1;

            await _db.SaveChangesAsync();

            return new LoanResponse().ToResponse(loan);
        }

        public async Task<List<LoanResponse>> GetMyLoansAsync()
        {
            var userId = _identity.GetUserId();

            if (userId == null)
                throw new Exception("Unauthorized");

            var loans = await _db.Loans
                .Where(x => x.UserId == userId.Value)
                .Include(x => x.Book)
                .Include(x => x.User)
                .OrderByDescending(x => x.LoanDate)
                .ToListAsync();
            await UpdateOverdueStatusAsync(loans);
            return loans.Select(x => new LoanResponse().ToResponse(x)).ToList();
        }

        public async Task<PagedHttpResponse<LoanResponse>> GetMyLoansPagedAsync(int? pageIndex, int? pageSize, string? status = null)
        {
            var userId = _identity.GetUserId();

            if (userId == null)
                throw new Exception("Unauthorized");
            var query = _db.Loans
                .Where(x => x.UserId == userId.Value)
                .Include(x => x.Book)
                .Include(x => x.User)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }
            var totalItems = await query.CountAsync();
            var allLoans = await query
                .OrderByDescending(x => x.LoanDate)
                .ToListAsync();
            
            await UpdateOverdueStatusAsync(allLoans);
            IEnumerable<Loan> pagedLoans = allLoans;
            if (pageIndex.HasValue && pageSize.HasValue && pageIndex > 0 && pageSize > 0)
            {
                pagedLoans = allLoans
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var loanResponses = pagedLoans.Select(x => new LoanResponse().ToResponse(x));

            return new PagedHttpResponse<LoanResponse>
            {
                TotalItems = totalItems,
                TotalPages = PaginationUtils.TotalPagesConversion(totalItems, pageSize),
                Items = loanResponses
            };
        }

        public async Task<LoanResponse?> ApproveLoanAsync(int loanId)
        {
            var loan = await _db.Loans
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == loanId);

            if (loan == null)
                return null;

            if (loan.Status != LoanStatus.Pending.ToString())
                throw new Exception("Only pending loans can be approved.");
            loan.Status = LoanStatus.Approved.ToString();
            loan.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            var notification = new Notification
            {
                UserId = loan.UserId,
                Title = "Phê duyệt mượn sách",
                Message = $"Yêu cầu mượn sách \"{loan.Book.Title}\" đã được phê duyệt.",
                Type = NotificationType.LoanApproved.ToString(),
                LoanId = loan.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();

            return new LoanResponse().ToResponse(loan);
        }
      
        public async Task<LoanResponse?> CancelLoanAsync(int loanId)
        {
            var loan = await _db.Loans
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == loanId);

            if (loan == null)
                return null;

            if (loan.Status != LoanStatus.Pending.ToString())
                throw new Exception("Only pending loans can be cancelled.");
            loan.Status = LoanStatus.Cancelled.ToString();
            loan.UpdatedAt = DateTime.UtcNow;
            loan.Book.StockQuantity += 1;

            await _db.SaveChangesAsync();

            return new LoanResponse().ToResponse(loan);
        }

        private async Task UpdateOverdueStatusAsync(List<Loan> loans)
        {
            bool updated = false;

            foreach (var loan in loans)
            {
                if (loan.Status == LoanStatus.Approved.ToString() &&
                    loan.DueDate.HasValue &&
                    DateTime.UtcNow > loan.DueDate.Value)
                {
                    loan.Status = LoanStatus.Overdue.ToString();
                    int overdueDays = (int)(DateTime.UtcNow - loan.DueDate.Value).TotalDays;
                    int fineAmount = overdueDays * 20000;
                    
                    loan.FineAmount = fineAmount;
                    loan.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
                else if (loan.Status == LoanStatus.Overdue.ToString() && 
                         loan.DueDate.HasValue)
                {
                    int overdueDays = (int)(DateTime.UtcNow - loan.DueDate.Value).TotalDays;
                    int fineAmount = overdueDays * 20000;
                    
                    if (loan.FineAmount != fineAmount)
                    {
                        loan.FineAmount = fineAmount;
                        loan.UpdatedAt = DateTime.UtcNow;
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                await _db.SaveChangesAsync();
            }
        }

        public async Task<LoanResponse?> PayFineAsync(int loanId)
        {
            var loan = await _db.Loans
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == loanId);

            if (loan == null)
                return null;

            if (!loan.DueDate.HasValue)
                throw new Exception("Loan does not have a due date.");

            if (loan.Status == LoanStatus.Paid.ToString())
            {
                return new LoanResponse().ToResponse(loan);
            }

            //Chỉ cho phép thanh toán khi status là Overdue hoặc Returned
            if (loan.Status != LoanStatus.Overdue.ToString() && loan.Status != LoanStatus.Returned.ToString())
                throw new Exception($"Cannot pay fine. Loan status is {loan.Status}. Only Overdue or Returned loans can pay fine.");
            if (loan.FineAmount <= 0)
                throw new Exception("No fine to pay for this loan.");
            if (loan.ReturnDate == null)
            {
                loan.ReturnDate = DateTime.UtcNow;
            }
            loan.Status = LoanStatus.Paid.ToString();
            loan.UpdatedAt = DateTime.UtcNow;
            
            await _db.SaveChangesAsync();
            
            return new LoanResponse().ToResponse(loan);
        }

        public async Task<LoanResponse?> ApprovePaymentAsync(int loanId)
        {
            var loan = await _db.Loans
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == loanId);

            if (loan == null)
                return null;

            if (loan.Status != LoanStatus.Paid.ToString())
                throw new Exception("Only paid loans can be approved for return. Loan status must be 'Paid'.");
            loan.Status = LoanStatus.Returned.ToString();
            loan.ReturnDate = DateTime.UtcNow; 
            loan.UpdatedAt = DateTime.UtcNow;
            if (loan.Book != null)
            {
                loan.Book.StockQuantity += 1;
            }

            await _db.SaveChangesAsync();

            var notification = new Notification
            {
                UserId = loan.UserId,
                Title = "Đã duyệt trả sách",
                Message = $"Yêu cầu trả sách \"{loan.Book?.Title}\" đã được duyệt. Bạn có thể tiếp tục mượn sách mới.",
                Type = NotificationType.PaymentApproved.ToString(),
                LoanId = loan.Id,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();

            return new LoanResponse().ToResponse(loan);
        }

        public async Task<LoanSummaryResponse> GetLoanSummaryByPeriodAsync(int year, int? month = null)
        {
            DateTime startDate, endDate;

            if (month.HasValue)
            {
                startDate = new DateTime(year, month.Value, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                startDate = new DateTime(year, 1, 1);
                endDate = new DateTime(year, 12, 31);
            }
            var loans = await _db.Loans
                .Where(x => x.LoanDate.HasValue)
                .ToListAsync();
            var filteredLoans = loans
                .Where(x => x.LoanDate.Value.ToUniversalTime().Date >= startDate.Date &&
                           x.LoanDate.Value.ToUniversalTime().Date <= endDate.Date)
                .ToList();
            var totalLoans = filteredLoans.Count;
            var approvingLoans = filteredLoans
                .Count(x => x.Status == LoanStatus.Approved.ToString() ||
                           x.Status == LoanStatus.Overdue.ToString() ||
                           x.Status == LoanStatus.Paid.ToString());
            var overdueLoans = filteredLoans
                .Count(x => x.Status == LoanStatus.Overdue.ToString() ||
                           x.Status == LoanStatus.Paid.ToString());

            return new LoanSummaryResponse
            {
                TotalLoans = totalLoans,
                ApprovingLoans = approvingLoans,
                OverdueLoans = overdueLoans
            };
        }
    }
}
                            