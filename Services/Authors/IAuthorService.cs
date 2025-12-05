using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Threading.Tasks;

namespace MyApi.Services.Authors
{
    public interface IAuthorService
    {
        Task<Author?> GetByIdAsync(Guid id);
        Task<AuthorCreateResponse?> CreateAuthorAsync(AuthorCreateRequest request);
        Task<AuthorUpdateResponse?> UpdateAuthorAsync(AuthorUpdateRequest request);
        Task<bool> DeleteAuthorAsync(Guid id);
        Task<PagedAuthorResponse> GetAllAuthorsAsync(int? pageIndex, int? pageSize);
    }
}
