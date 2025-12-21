using MyApi.Model.Request;
using MyApi.Model.Response;
using MyApi.Entities;
using System.Threading.Tasks;

namespace MyApi.Services.Categories
{
    public interface ICategoryService
    {
        // Get category by Id
        Task<Category?> GetById(int id);

        // Create category
        Task<CategoryCreateResponse?> CreateCategoryAsync(CategoryCreateRequest request);

        // Update category
        Task<CategoryUpdateResponse?> UpdateCategoryAsync(CategoryUpdateRequest request);

        // Delete category
        Task<bool> DeleteCategoryAsync(int id);

        // ⭐ PAGINATION CHUẨN
        Task<CategoryPagedResponse> GetAllCategoriesAsync(int page, int pageSize);
    }
}
