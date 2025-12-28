using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Services.Loans;
using MyApi.Services.Users;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController  : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize("AdminOnly")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser(int? pageIndex, int? pageSize)
        {
            var result = await _userService.GetAllUsersAsync(pageIndex, pageSize);
            return Ok(result);
        }

        [Authorize("AdminOnly")]
        [HttpPut]
        public async Task<IActionResult> ChangePasswordUser(Guid userId, string newPassword)
        {
            var result = await _userService.ChangePasswordUser(userId, newPassword);
            return Ok(result);
        }


    }
}
