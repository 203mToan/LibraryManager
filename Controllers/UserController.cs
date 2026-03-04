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
        [HttpGet]
        public async Task<IActionResult> GetAllUser(int? pageIndex, int? pageSize)
        {
            var result = await _userService.GetAllUsersAsync(pageIndex, pageSize);
            return Ok(result);
        }
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
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordUser(Guid userId, string newPassword)
        {
            var result = await _userService.ChangePasswordUser(userId, newPassword);
            if (result == null)
                return NotFound();

            return Ok("Password changed successfully");
        }
        [AllowAnonymous]
        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { success = false, message = "Username is required" });

            var exists = await _userService.IsUsernameExistsAsync(username);

            return Ok(new
            {
                success = true,
                exists,
                message = exists ? "Username đã được sử dụng" : "Username có thể sử dụng"
            });
        }
        [AllowAnonymous]
        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { success = false, message = "Email is required" });

            var exists = await _userService.IsEmailExistsAsync(email);

            return Ok(new
            {
                success = true,
                exists,
                message = exists ? "Email đã được sử dụng" : "Email có thể sử dụng"
            });
        }
        [AllowAnonymous]
        [HttpGet("check-phone/{phone}")]
        public async Task<IActionResult> CheckPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(new { success = false, message = "Phone is required" });

            var exists = await _userService.IsPhoneExistsAsync(phone);

            return Ok(new
            {
                success = true,
                exists,
                message = exists ? "Số điện thoại đã được sử dụng" : "Số điện thoại có thể sử dụng"
            });
        }
        [AllowAnonymous]
        [HttpGet("check-nationalid/{nationalId}")]
        public async Task<IActionResult> CheckNationalId(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return BadRequest(new { success = false, message = "NationalId is required" });

            var exists = await _userService.IsNationalIdExistsAsync(nationalId);

            return Ok(new
            {
                success = true,
                exists,
                message = exists ? "CCCD/CMND đã được sử dụng" : "CCCD/CMND có thể sử dụng"
            });
        }
    }
}
