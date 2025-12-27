using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.FavoriteBooks;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyApi.Controllers
{
    [Route("api/favorite")]
    [ApiController]
    [Authorize]
    public class FavoriteBookController : ControllerBase
    {
        private readonly IFavoriteBookService _favoriteBookService;

        public FavoriteBookController(IFavoriteBookService favoriteBookService)
        {
            _favoriteBookService = favoriteBookService;
        }

        // ➕ ADD FAVORITE
        [HttpPost]
        public async Task<IActionResult> Add(FavoriteBookRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _favoriteBookService.AddAsync(userId, request.BookId);

            if (!success)
                return BadRequest("Book already in favorites");

            return Ok();
        }

        // ❌ REMOVE FAVORITE
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> Remove(int bookId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _favoriteBookService.RemoveAsync(userId, bookId);

            if (!success)
                return NotFound();

            return Ok();
        }
    }
}
