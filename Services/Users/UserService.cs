using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Response;
using System;
namespace MyApi.Services.Users
{

    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _db.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> ValidateUser(string username, string password)
        {
            var user = await GetByUsername(username);
            if (user == null) return null;

            var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            // Nếu dùng password hash thì thay bằng PasswordHasher
            if (!isValid)
                return null;

            return user;
        }

        public async Task SaveRefreshToken(Guid userId, string refreshToken)
        {
            var rt = new RefreshTokens
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                IsUsed = false
            };

            await _db.RefreshTokens.AddAsync(rt);
            await _db.SaveChangesAsync();
        }

        public async Task<RefreshTokens?> GetRefreshToken(string token)
        {
            return await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == token && !x.IsUsed);
        }

        public async Task MarkRefreshTokenAsUsed(RefreshTokens token)
        {
            token.IsUsed = true;
            token.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task ReplaceRefreshToken(RefreshTokens oldToken, string newToken)
        {
            await MarkRefreshTokenAsUsed(oldToken);

            await SaveRefreshToken(oldToken.UserId, newToken);
        }

        public Task<User?> ChangePasswordUser(Guid userId, string newPassword)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return Task.FromResult<User?>(null);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _db.SaveChanges();
            return Task.FromResult<User?>(user);
        }

        public Task<PagedUserResponse> GetAllUsersAsync(int? pageIndex, int? pageSize)
        {
            var query = _db.Users.Include(u => u.Role).AsQueryable();
            var totalItems = query.Count();
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }
            var users = query.ToList();
            var userResponses = users.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                DateOfBirth = u.DateOfBirth,
            });
            var pagedResponse = new PagedUserResponse(userResponses,totalItems, pageSize);
            return Task.FromResult(pagedResponse);
        }
    }

}
