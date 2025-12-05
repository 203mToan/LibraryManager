using MyApi.Entities;

namespace MyApi.Model.Request
{
    public class LoanRequest
    {
        public Guid UserId { get; set; }
        public int BookId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }

        public Loan ToEntity()
        {
            return new Loan
            {
                UserId = this.UserId,
                BookId = this.BookId,
                LoanDate = this.LoanDate,
                DueDate = this.DueDate,
                Status = LoanStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
