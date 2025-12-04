namespace MyApi.Utils
{
    public static class CaculationFineAmount
    {
        public static int CalculateFineAmount(DateTime? dueDate, DateTime returnDate)
        {
            if (returnDate <= dueDate)
            {
                return 0; // No fine if returned on or before due date
            }
            int overdueDays = (returnDate - (DateTime)dueDate).Days;
            int fineAmount = overdueDays * 20000;
            return fineAmount;
        }
    }
}
