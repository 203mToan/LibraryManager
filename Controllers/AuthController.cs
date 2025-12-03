using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services;
using MyApi.Services.Users;


namespace MyApi.Controllers
{
    public class AuthController : ControllerBase
    {
        private IUserService _userService;
        private readonly TokenService _tokenGen;
        public AuthController(IUserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenGen = tokenService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            var user = await _userService.ValidateUser(model.UserName, model.Password);
            if (user == null) return Unauthorized("Invalid credentials");

            var accessToken = _tokenGen.GenerateAccessToken(user);
            var refreshToken = _tokenGen.GenerateRefreshToken();

            await _userService.SaveRefreshToken(user.Id, refreshToken);
            var loginResponse = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(loginResponse);
        }
    }
}
