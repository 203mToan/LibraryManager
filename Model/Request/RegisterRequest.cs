namespace MyApi.Model.Request
{
    public class RegisterRequest
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public string NationalId { get; set; } = null!;

        // User sẽ nhập Password → backend sẽ Hash
        public string Password { get; set; } = null!;
    }
}
