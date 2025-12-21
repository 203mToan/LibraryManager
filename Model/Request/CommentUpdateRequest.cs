namespace MyApi.Model.Request
{
    public class CommentUpdateRequest
    {
        public int Rating { get; set; }
        public string? Content { get; set; }
    }
}
