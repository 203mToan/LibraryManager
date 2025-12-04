namespace MyApi
{
    public class PaginationUtils
    {
        public static int TotalPagesConversion(int totalItems, int? pageSize)
        {
            if (!pageSize.HasValue)
            {
                return 0;
            }

            if (totalItems % pageSize.Value != 0)
            {
                return totalItems / pageSize.Value + 1;
            }

            return totalItems / pageSize.Value;
        }
    }
}
