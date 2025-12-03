using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
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
    }

}
