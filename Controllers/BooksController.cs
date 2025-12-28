using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.Books;
using System.Threading.Tasks;

namespace MyApi.Controllers
{
    [Route("api/book")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: api/book?page=1&pageSize=10
        [Authorize("AdminOrUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllBooks(
            [FromQuery] int? categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10

        )
        {
            var result = await _bookService.GetAllBooksAsync(page, pageSize, categoryId);
            return Ok(result);
        }

        // POST: api/book
        [Authorize("AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateBook(
            [FromBody] BookCreateRequest model
        )
        {
            var result = await _bookService.CreateBookAsync(model);

            if (result == null)
                return BadRequest(new { message = "Failed to create book" });

            return Ok(result);
        }

        // PUT: api/book
        [Authorize("AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> UpdateBook(
            [FromBody] BookUpdateRequest model
        )
        {
            var result = await _bookService.UpdateBookAsync(model);

            if (result == null)
                return NotFound(new { message = "Book not found" });

            return Ok(result);
        }

        // DELETE: api/book/{id}
        [Authorize("AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var deleted = await _bookService.DeleteBookAsync(id);

            if (!deleted)
                return NotFound(new { message = "Book not found" });

            return Ok(new { message = "Book deleted successfully" });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _bookService.GetByIdAsync(id);
            return Ok(book);
        }
    }
}
