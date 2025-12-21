using System;
using System.Collections.Generic;

namespace MyApi.Model.Response
{
    public class CategoryPagedResponse
    {
        public IEnumerable<CategoryResponse> Items { get; }
        public int TotalItems { get; }
        public int? PageSize { get; }
        public int TotalPages => PageSize.HasValue && PageSize.Value > 0
            ? (int)Math.Ceiling((double)TotalItems / PageSize.Value)
            : 1;

        public CategoryPagedResponse(IEnumerable<CategoryResponse> items, int totalItems, int? pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageSize = pageSize;
        }
    }
}
