using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.Categories;

namespace MyApi.Services.Categories
{
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Authorize("AdminOrUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(int? pageIndex, int? pageSize)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(pageIndex, pageSize);
            return Ok(categories);
        }

        [Authorize("AdminOnly")]
        [HttpPost]
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

        [Authorize("AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory(CategoryUpdateRequest model)
        {
            var result = await _categoryService.UpdateCategoryAsync(model);

            if (result == null)
                return NotFound("Category not found");

            var response = new
            {
                Id = result.Id,
                Message = result.Message
            };

            return Ok(response);
        }

        [Authorize("AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);

            if (!result)
                return NotFound(new { message = "Category not found" });

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}
