using System;
using System.Collections.Generic;

namespace MyApi.Model.Response
{
    public class PagedAuthorResponse
    {
        public IEnumerable<AuthorResponse> Items { get; }
        public int TotalItems { get; }
        public int? PageSize { get; }
        public int TotalPages => PageSize.HasValue && PageSize.Value > 0
            ? (int)Math.Ceiling((double)TotalItems / PageSize.Value)
            : 1;

        public PagedAuthorResponse(IEnumerable<AuthorResponse> items, int totalItems, int? pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageSize = pageSize;
        }
    }
}
