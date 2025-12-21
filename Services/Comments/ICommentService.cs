using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApi.Services.Comments
{
    public interface ICommentService
    {
        Task<CommentPagedResponse<CommentResponse>> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<CommentResponse>> GetByBookIdAsync(int bookId);

        Task<CommentResponse> CreateAsync(Guid userId, CommentCreateRequest request);
        Task<bool> UpdateAsync(Guid id, Guid userId, CommentUpdateRequest request);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}
