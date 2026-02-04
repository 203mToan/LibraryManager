using MyApi.Entities;
using MyApi.Model.Request;
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

        // ✅ NEW
        Task<UserResponse?> GetUserByIdAsync(Guid userId);
        Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUserRequest request);

        // ✅ Check duplicate fields
        Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null);
        Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null);
        Task<bool> IsPhoneExistsAsync(string phone, Guid? excludeUserId = null);
        Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeUserId = null);
    }
}
