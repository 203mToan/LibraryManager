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
            int fine = loan.FineAmount;
            
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
                FineAmount = fine
            };
        }
    }
}
