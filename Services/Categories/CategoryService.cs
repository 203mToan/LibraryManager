using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;

namespace MyApi.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Category?> GetById(int id)
        {
            return await _db.Categories
                .Include(c => c.Books) // include navigation nếu cần
                .FirstOrDefaultAsync(c => c.Id == id);
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

        public async Task<CategoryUpdateResponse?> UpdateCategoryAsync(CategoryUpdateRequest request)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id);
            if (category == null)
                return null;

            if (request.Name != null) category.Name = request.Name;
            if (request.Description != null) category.Description = request.Description;

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

            // Optionally: prevent delete if has books (business rule)
            // if (category.Books != null && category.Books.Any()) return false;

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedCategoryResponse> GetAllCategoriesAsync(int? pageIndex, int? pageSize)
        {
            var query = _db.Categories.AsQueryable();

            var totalItems = await query.CountAsync();

            // nếu pageIndex/pageSize không được cung cấp, trả toàn bộ
            if (!pageIndex.HasValue || !pageSize.HasValue || pageSize.Value <= 0)
            {
                var all = await query
                    .Include(c => c.Books)
                    .ToListAsync();

                var itemsAll = all.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    BookCount = c.Books?.Count
                }).ToList();

                return new PagedCategoryResponse(itemsAll, totalItems, pageSize);
            }

            var skip = (pageIndex.Value - 1) * pageSize.Value;
            var paged = await query
                .Include(c => c.Books)
                .Skip(skip)
                .Take(pageSize.Value)
                .ToListAsync();

            var items = paged.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                BookCount = c.Books?.Count
            }).ToList();

            return new PagedCategoryResponse(items, totalItems, pageSize);
        }
    }
}
