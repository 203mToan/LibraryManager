using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<Category?> GetById(int id)
        {
            return await _db.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CategoryCreateResponse?> CreateCategoryAsync(CategoryCreateRequest request)
        {
            if (request == null) return null;

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

        public async Task<CategoryUpdateResponse?> UpdateCategoryAsync(CategoryUpdateRequest request)
        {
            if (request == null) return null;

            var category = await _db.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id);

            if (category == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Name))
                category.Name = request.Name;

            if (request.Description != null)
                category.Description = request.Description;

            category.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return new CategoryUpdateResponse
            {
                Id = category.Id,
                Message = "Category updated successfully"
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return true;
        }

        // ⭐ PAGINATION CHUẨN – KHÔNG TRẢ ALL DATA
        public async Task<PagedCategoryResponse> GetAllCategoriesAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Categories
                .Include(c => c.Books)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    BookCount = c.Books.Count
                })
                .ToListAsync();

            return new PagedCategoryResponse(
                items,
                totalItems,
                pageSize
            );
        }
    }
}
