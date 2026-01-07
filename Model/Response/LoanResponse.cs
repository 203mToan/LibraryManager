using MyApi.Entities;
using MyApi.Utils;

namespace MyApi.Model.Response
{
    public class LoanResponse
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public DateTime? LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? FineAmount { get; set; }
        public string? Status { get; set; }  // Pending / Approved / Returned / Overdue / Cancelled / Paid

        public LoanResponse ToResponse(Loan loan)
        {
            dynamic fine = 0;
            
            // Tính tiền phạt cho các trạng thái Approved hoặc Overdue (chưa thanh toán)
            if((loan.Status == LoanStatus.Approved.ToString() || loan.Status == LoanStatus.Overdue.ToString()) 
               && loan.DueDate.HasValue && loan.DueDate < DateTime.UtcNow)
            {
                fine = CaculationFineAmount.CalculateFineAmount(loan.DueDate, DateTime.UtcNow);
            }
            
            // Nếu đã thanh toán (Paid) hoặc có FineAmount trong DB, lấy số tiền từ database
            if (loan.Status == LoanStatus.Paid.ToString() || loan.FineAmount > 0)
            {
                fine = loan.FineAmount;
            }
            
            return new LoanResponse
            {
                Id = loan.Id,
                UserId = loan.UserId,
                UserName = loan.User.FullName,
                BookName = loan.Book.Title,
                BookId = loan.BookId,
                LoanDate = loan.LoanDate,
                DueDate = loan.DueDate,
                ReturnDate = loan.ReturnDate,
                Status = loan.Status,
                FineAmount = fine == 0 ? 0 : fine
            };
        }
    }
}
