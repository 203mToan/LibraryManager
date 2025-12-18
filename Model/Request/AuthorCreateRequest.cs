namespace MyApi.Model.Request
{
    public class AuthorCreateRequest
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Nationality { get; set; }
        public int? BirthYear { get; set; }
    }
}
