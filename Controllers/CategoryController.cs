using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request.Category;
using MyApi.Services.Categories;

namespace MyApi.Services.Categories
{
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("createCategory")]
        public async Task<IActionResult> CreateCategory(CategoryCreateRequest model)
        {
            var result = await _categoryService.CreateCategoryAsync(model);

            if (result == null)
                return BadRequest("Failed to create category");

            var response = new
            {
                Id = result.Id,
                Message = result.Message
            };

            return Ok(response);
        }
    }
}
