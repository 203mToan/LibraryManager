using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services;

namespace MyApi.Services.Books
{
    [Route("api/book")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }
        [Authorize("AdminOrUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllBooks(int? pageIndex, int? pageSize)
        {
            var books = await _bookService.GetAllBooksAsync(pageIndex, pageSize);
            return Ok(books);
        }
        [Authorize("AdminOnly")]
        [HttpPost]
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

        [Authorize("AdminOnly")]
        [HttpPut]
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

        [Authorize("AdminOnly")]
        [HttpDelete]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);

            if (!result)
                return NotFound(new { message = "Book not found" });

            return Ok(new { message = "Book deleted successfully" });
        }

    }
}
