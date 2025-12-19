using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.Categories;
using System.Threading.Tasks;

namespace MyApi.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/category?page=1&pageSize=10
        [Authorize("AdminOrUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await _categoryService.GetAllCategoriesAsync(page, pageSize);
            return Ok(result);
        }

        // POST: api/category
        [Authorize("AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryCreateRequest model
        )
        {
            var result = await _categoryService.CreateCategoryAsync(model);

            if (result == null)
                return BadRequest(new { message = "Failed to create category" });

            return Ok(result);
        }

        // PUT: api/category
        [Authorize("AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateCategory(
            [FromBody] CategoryUpdateRequest model
        )
        {
            var result = await _categoryService.UpdateCategoryAsync(model);

            if (result == null)
                return NotFound(new { message = "Category not found" });

            return Ok(result);
        }

        // DELETE: api/category/{id}
        [Authorize("AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);

            if (!deleted)
                return NotFound(new { message = "Category not found" });

            return Ok(new { message = "Category deleted successfully" });
        }
    }
}
