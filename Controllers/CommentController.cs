using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.Comments;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyApi.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // ✅ ADMIN – GET ALL COMMENT (PHÂN TRANG)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            var comments = await _commentService.GetAllAsync(page, pageSize);
            return Ok(comments);
        }

        // PUBLIC – GET COMMENT BY BOOK
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetByBook(int bookId)
        {
            var comments = await _commentService.GetByBookIdAsync(bookId);
            return Ok(comments);
        }

        // USER – CREATE COMMENT
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CommentCreateRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _commentService.CreateAsync(userId, request);
            return Ok(result);
        }

        // USER – UPDATE COMMENT
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CommentUpdateRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _commentService.UpdateAsync(id, userId, request);
            return success ? Ok() : Forbid();
        }

        // USER – DELETE COMMENT
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _commentService.DeleteAsync(id, userId);
            return success ? Ok() : Forbid();
        }
    }
}
