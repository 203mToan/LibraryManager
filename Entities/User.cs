namespace MyApi.Entities
{
    public class User : BaseEntity<Guid>
    {

        public string? FullName { get; set; }

        public string? Email { get; set; }
        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? NationalId { get; set; } = null!;

        public string? PasswordHash { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<Loan> Loans { get; set; }


    }

}
