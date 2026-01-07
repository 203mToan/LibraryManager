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

            // Load user's loans into memory and check overdue in-memory to avoid PostgreSQL timestamptz vs timestamp translation issues
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
            
            // Kiểm tra nếu trả muộn
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

            // Increase back the book quantity
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

            // Cập nhật trạng thái
            loan.Status = LoanStatus.Approved.ToString();
            loan.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // ✅ Tạo notification cho user
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

            // Cập nhật trạng thái
            loan.Status = LoanStatus.Cancelled.ToString();
            loan.UpdatedAt = DateTime.UtcNow;
            
            // Hoàn lại số lượng sách
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
                    // ✅ Khi chuyển sang Overdue, tính tiền phạt
                    loan.Status = LoanStatus.Overdue.ToString();
                    
                    // Tính tiền phạt: số ngày trễ × 20000đ
                    int overdueDays = (int)(DateTime.UtcNow - loan.DueDate.Value).TotalDays;
                    int fineAmount = overdueDays * 20000;
                    
                    loan.FineAmount = fineAmount;
                    loan.UpdatedAt = DateTime.UtcNow;
                    updated = true;
                }
                // ✅ Nếu đã là Overdue, cập nhật tiền phạt mỗi lần query
                else if (loan.Status == LoanStatus.Overdue.ToString() && 
                         loan.DueDate.HasValue)
                {
                    // Cập nhật tiền phạt theo số ngày trễ hiện tại
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

            // ✅ Nếu đã thanh toán (Paid), trả về thông tin
            if (loan.Status == LoanStatus.Paid.ToString())
            {
                return new LoanResponse().ToResponse(loan);
            }

            // ✅ Chỉ cho phép thanh toán khi status là Overdue hoặc Returned
            if (loan.Status != LoanStatus.Overdue.ToString() && loan.Status != LoanStatus.Returned.ToString())
                throw new Exception($"Cannot pay fine. Loan status is {loan.Status}. Only Overdue or Returned loans can pay fine.");

            // ✅ Kiểm tra có tiền phạt không
            if (loan.FineAmount <= 0)
                throw new Exception("No fine to pay for this loan.");

            // ✅ Lưu thời gian trả sách thực tế khi user ấn thanh toán
            if (loan.ReturnDate == null)
            {
                loan.ReturnDate = DateTime.UtcNow;
            }

            // ✅ Cập nhật trạng thái thành Paid - chờ admin duyệt
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
                throw new Exception("Only paid loans can be approved. Loan status must be 'Paid'.");

            // ✅ Lưu doanh thu phạt vào bảng FinePayments
            int fineAmount = loan.FineAmount;
            if (fineAmount > 0)
            {
                var finePayment = new FinePayment
                {
                    UserId = loan.UserId,
                    LoanId = loan.Id,
                    Amount = fineAmount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = "System",
                    Description = $"Fine payment for book: {loan.Book.Title}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _db.FinePayments.AddAsync(finePayment);
            }

            // ✅ Admin duyệt thanh toán → chuyển về Returned và xóa tiền phạt
            loan.Status = LoanStatus.Returned.ToString();
            loan.FineAmount = 0; // Xóa tiền phạt
            loan.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // ✅ Tạo notification cho user
            var notification = new Notification
            {
                UserId = loan.UserId,
                Title = "Phê duyệt thanh toán phạt",
                Message = $"Thanh toán phạt trễ hạn cho sách \"{loan.Book.Title}\" đã được duyệt.",
                Type = NotificationType.PaymentApproved.ToString(),
                LoanId = loan.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();

            return new LoanResponse().ToResponse(loan);
        }
    }
}
                            