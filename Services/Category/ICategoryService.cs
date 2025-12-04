using MyApi.Model.Request.Category;
using MyApi.Model.Response.Category;

namespace MyApi.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryCreateResponse?> CreateCategoryAsync(CategoryCreateRequest request);
    }
}
