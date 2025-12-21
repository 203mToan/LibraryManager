namespace MyApi.Model.Request
{
    public class CommentCreateRequest
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string? Content { get; set; }
    }
}
