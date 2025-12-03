using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services;

namespace MyApi.Services.Books
{
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBook(BookCreateRequest model)
        {
            var result = await _bookService.CreateBookAsync(model);

            if (result == null)
                return BadRequest("Failed to create book");

            var response = new
            {
                Id = result.Id,
                Message = result.Message
            };

            return Ok(response);
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdateBook(BookUpdateRequest model)
        {
            var result = await _bookService.UpdateBookAsync(model);

            if (result == null)
                return NotFound("Book not found");

            var response = new
            {
                Id = result.Id,
                Message = result.Message
            };

            return Ok(response);
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);

            if (!result)
                return NotFound(new { message = "Book not found" });

            return Ok(new { message = "Book deleted successfully" });
        }

    }
}
