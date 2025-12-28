using MyApi.Entities;
using MyApi.Model.Response;

namespace MyApi.Services.Users
{
    public interface IUserService
    {
        Task<User?> GetByUsername(string username);
        Task<User?> ValidateUser(string username, string password);

        Task SaveRefreshToken(Guid userId, string refreshToken);
        Task<RefreshTokens?> GetRefreshToken(string token);
        Task ReplaceRefreshToken(RefreshTokens oldToken, string newToken);
        Task<User?> ChangePasswordUser(Guid userId, string newPassword);
        Task<PagedUserResponse> GetAllUsersAsync(int? pageIndex, int? pageSize);
    }
}
