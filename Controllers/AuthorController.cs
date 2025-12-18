using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Model.Response;
using MyApi.Services.Authors;
using System;
using System.Threading.Tasks;

namespace MyApi.Controllers
{
    [Route("api/author")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: api/author?page=1&pageSize=10
        [Authorize("AdminOrUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllAuthors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await _authorService.GetAllAuthorsAsync(page, pageSize);
            return Ok(result);
        }

        // GET: api/author/{id}
        [Authorize("AdminOrUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var author = await _authorService.GetByIdAsync(id);
            if (author == null)
                return NotFound(new { message = "Author not found" });

            return Ok(new AuthorResponse
            {
                Id = author.Id,
                FullName = author.FullName,
                Bio = author.Bio,
                Nationality = author.Nationality,
                BirthYear = author.BirthYear,
                BookCount = author.Books?.Count ?? 0
            });
        }

        // POST: api/author
        [Authorize("AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateRequest model)
        {
            var result = await _authorService.CreateAuthorAsync(model);
            if (result == null)
                return BadRequest(new { message = "Failed to create author" });

            return Ok(result);
        }

        // PUT: api/author
        [Authorize("AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateAuthor([FromBody] AuthorUpdateRequest model)
        {
            var result = await _authorService.UpdateAuthorAsync(model);
            if (result == null)
                return NotFound(new { message = "Author not found" });

            return Ok(result);
        }

        // DELETE: api/author/{id}
        [Authorize("AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var deleted = await _authorService.DeleteAuthorAsync(id);
            if (!deleted)
                return NotFound(new { message = "Author not found or cannot be deleted" });

            return Ok(new { message = "Author deleted successfully" });
        }
    }
}
