using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request.Category;
using MyApi.Model.Response.Category;

namespace MyApi.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CategoryCreateResponse?> CreateCategoryAsync(CategoryCreateRequest request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();

            return new CategoryCreateResponse
            {
                Id = category.Id,
                Message = "Category created successfully"
            };
        }
    }
}
