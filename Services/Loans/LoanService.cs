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
            var loanOverDue = await _db.Loans
                .Where(x => x.ReturnDate >= DateTime.Now || x.Status == LoanStatus.Overdue.ToString())
                .FirstOrDefaultAsync();
            if(loanOverDue != null)
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
            var loans = new List<Loan> { loan! };
            await UpdateOverdueStatusAsync(loans);

            if (loan == null) return null;

            return new LoanResponse().ToResponse(loan);
        }

        public async Task<LoanResponse?> ReturnBookAsync(int id, DateTime returnDate)
        {
            var loan = await _db.Loans
                .Include(x => x.User)
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (loan == null) return null;

            if (loan.Status == LoanStatus.Returned.ToString())
                throw new Exception("Book already returned");

            loan.ReturnDate = returnDate;
            loan.Status = LoanStatus.Returned.ToString();
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

            return new LoanResponse().ToResponse(loan);
        }
        private async Task UpdateOverdueStatusAsync(List<Loan> loans)
        {
            bool updated = false;

            foreach (var loan in loans)
            {
                if (loan.Status == LoanStatus.Approved.ToString() &&
                    loan.DueDate.HasValue &&
                    DateTime.Now > loan.DueDate.Value)
                {
                    loan.Status = LoanStatus.Overdue.ToString();
                    loan.UpdatedAt = DateTime.UtcNow;
                    updated = true;
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
            loan.Status = LoanStatus.Returned.ToString();
            loan.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return new LoanResponse().ToResponse(loan);
        }

    }
}
