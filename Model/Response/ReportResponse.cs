namespace MyApi.Model.Response
{
    public class LoanTrendResponse
    {
        public string Period { get; set; } // "week", "month", "year"
        public List<TrendDataPoint> Data { get; set; } = new();
    }
    public class TrendDataPoint
    {
        public string Label { get; set; } // "Tu?n 1", "Tháng 1", "2026"
        public int LoanCount { get; set; }
        public int ReturnCount { get; set; }
    }
    public class CategoryDistributionResponse
    {
        public List<CategoryDistributionData> Categories { get; set; } = new();
        public int TotalLoans { get; set; }
    }

    public class CategoryDistributionData
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int LoanCount { get; set; }
        public decimal Percentage { get; set; }
    }
    public class TopUserResponse
    {
        public List<UserStatistics> Users { get; set; } = new();
    }

    public class UserStatistics
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int LoanCount { get; set; }
        public int ReturnedCount { get; set; }
        public int OverdueCount { get; set; }
    }
    public class TopBookResponse
    {
        public List<BookStatistics> Books { get; set; } = new();
    }

    public class BookStatistics
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string CategoryName { get; set; }
        public int LoanCount { get; set; }
        public int ReturnedCount { get; set; }
    }
}
