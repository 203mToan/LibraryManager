using System;

namespace MyApi.Model.Request
{
    public class AuthorUpdateRequest
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? National { get; set; }
        public int? BirthYear { get; set; }
    }
}
