namespace MyApi.Model.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }

        public string? Email { get; set; }
        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? NationalId { get; set; } = null!;

    }

    public class PagedUserResponse : PagedHttpResponse<UserResponse>
    {
        public PagedUserResponse(
            IEnumerable<UserResponse> items,
            int totalItems,
            int? pageSize
        )
        {
            TotalItems = totalItems;
            TotalPages = PaginationUtils.TotalPagesConversion(totalItems, pageSize);
            Items = items;
        }
    }
}
