using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services.Users;
using System.Security.Claims;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // =========================
        // GET ALL USERS (ADMIN)
        // =========================
        //[Authorize("AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser(int? pageIndex, int? pageSize)
        {
            var result = await _userService.GetAllUsersAsync(pageIndex, pageSize);
            return Ok(result);
        }

        // =========================
        // GET CURRENT USER PROFILE
        // GET: api/user/me
        // =========================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // =========================
        // UPDATE CURRENT USER PROFILE
        // PUT: api/user/me
        // =========================
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = Guid.Parse(userIdClaim);
            var updatedUser = await _userService.UpdateUserAsync(userId, request);

            if (updatedUser == null)
                return NotFound();

            return Ok(updatedUser);
        }

        // =========================
        // CHANGE PASSWORD
        // PUT: api/user/change-password
        // =========================
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordUser(Guid userId, string newPassword)
        {
            var result = await _userService.ChangePasswordUser(userId, newPassword);
            if (result == null)
                return NotFound();

            return Ok("Password changed successfully");
        }
    }
}
