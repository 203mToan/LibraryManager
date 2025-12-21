using Microsoft.AspNetCore.Mvc;
using MyApi.Model.Request;
using MyApi.Services;
using MyApi.Services.Authors;
using MyApi.Services.Users;


namespace MyApi.Controllers
{
    public class AuthController : ControllerBase
    {
        private IUserService _userService;
        private readonly TokenService _tokenGen;
        private readonly IAuthorService _authorService;
        public AuthController(IUserService userService, TokenService tokenService, IAuthorService authorService)
        {
            _userService = userService;
            _tokenGen = tokenService;
            _authorService = authorService;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            var existingUser = await _userService.GetByUsername(model.UserName);
            if (existingUser != null)
            {
                return BadRequest("Username already exists");
            }
            var user = await _authorService.RegisterAsync(model);
            return Ok(user);
        }
    }
}
