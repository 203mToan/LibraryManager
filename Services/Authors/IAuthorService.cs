using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Threading.Tasks;

namespace MyApi.Services.Authors
{
    public interface IAuthorService
    {
        // Get author by Id
        Task<Author?> GetByIdAsync(Guid id);

        // Create new author
        Task<AuthorCreateResponse?> CreateAuthorAsync(AuthorCreateRequest request);

        // Update author
        Task<AuthorUpdateResponse?> UpdateAuthorAsync(AuthorUpdateRequest request);

        // Delete author
        Task<bool> DeleteAuthorAsync(Guid id);

        // ⭐ PAGINATION CHUẨN (page, pageSize)
        Task<PagedAuthorResponse> GetAllAuthorsAsync(int page, int pageSize);
    }
}
