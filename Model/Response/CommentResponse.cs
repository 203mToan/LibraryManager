using System;

namespace MyApi.Model.Response
{
    public class CommentResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public string? UserName { get; set; }

        public int BookId { get; set; }
        public string? BookTitle { get; set; }

        public int Rating { get; set; }
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
