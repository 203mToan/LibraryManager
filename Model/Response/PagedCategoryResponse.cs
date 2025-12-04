using System;
using System.Collections.Generic;

namespace MyApi.Model.Response
{
    public class PagedCategoryResponse
    {
        public IEnumerable<CategoryResponse> Items { get; }
        public int TotalItems { get; }
        public int? PageSize { get; }
        public int TotalPages => PageSize.HasValue && PageSize.Value > 0
            ? (int)Math.Ceiling((double)TotalItems / PageSize.Value)
            : 1;

        public PagedCategoryResponse(IEnumerable<CategoryResponse> items, int totalItems, int? pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageSize = pageSize;
        }
    }
}
