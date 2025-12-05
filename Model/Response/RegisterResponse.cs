namespace MyApi.Model.Response
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
