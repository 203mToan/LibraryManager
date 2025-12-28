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

        // ➕ ADD FAVORITE (route: POST /api/favorite/{id})
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Add(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _favoriteBookService.AddAsync(userId, id);

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

            if (success == false)
                return NotFound();

            return Ok();
        }

        // ✅ CHECK IF A BOOK IS FAVORITED (GET /api/favorite/{bookId}/is-favorited)
        [HttpGet("{bookId:int}/is-favorited")]
        public async Task<IActionResult> IsFavorited(int bookId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isFavorited = await _favoriteBookService.IsFavoritedAsync(userId, bookId);
            return Ok(isFavorited);
        }

        // ⭐ GET MY FAVORITES (PAGINATION OBJECT)
        [HttpGet("me")]
        public async Task<IActionResult> GetMyFavorites(int page = 1, int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _favoriteBookService.GetMyFavoritesAsync(userId, page, pageSize);
            return Ok(result);
        }
    }
}