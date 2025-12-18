using System;

namespace MyApi.Model.Response
{
    public class AuthorResponse
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Nationality { get; set; }
        public int? BirthYear { get; set; }
        public int? BookCount { get; set; }
    }
}
