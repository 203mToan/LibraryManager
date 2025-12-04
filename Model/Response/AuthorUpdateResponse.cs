using System;

namespace MyApi.Model.Response
{
    public class AuthorUpdateResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
