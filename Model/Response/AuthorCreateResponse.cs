using System;

namespace MyApi.Model.Response
{
    public class AuthorCreateResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
