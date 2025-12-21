using System.Collections.Generic;

namespace MyApi.Model.Response
{
    public class CommentPagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
