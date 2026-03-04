using MyApi.Model.Request;
using MyApi.Model.Response;
using MyApi.Entities;
using System.Threading.Tasks;

namespace MyApi.Services.Categories
{
    public interface ICategoryService
    {
        Task<Category?> GetById(int id);
        Task<CategoryCreateResponse?> CreateCategoryAsync(CategoryCreateRequest request);
        Task<CategoryUpdateResponse?> UpdateCategoryAsync(CategoryUpdateRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<CategoryPagedResponse> GetAllCategoriesAsync(int page, int pageSize);
    }
}
