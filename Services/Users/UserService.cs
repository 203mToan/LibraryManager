using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Request;
using MyApi.Model.Response;

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
            return isValid ? user : null;
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

        public async Task ReplaceRefreshToken(RefreshTokens oldToken, string newToken)
        {
            oldToken.IsUsed = true;
            oldToken.RevokedAt = DateTime.UtcNow;

            await SaveRefreshToken(oldToken.UserId, newToken);
        }

        public async Task<User?> ChangePasswordUser(Guid userId, string newPassword)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<PagedUserResponse> GetAllUsersAsync(int? pageIndex, int? pageSize)
        {
            var query = _db.Users.Include(u => u.Role).AsQueryable();
            var totalItems = await query.CountAsync();

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query
                    .Skip((pageIndex.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }

            var users = await query.ToListAsync();

            var userResponses = users.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                UserName = u.UserName,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                DateOfBirth = u.DateOfBirth,
                Gender = u.Gender,
                NationalId = u.NationalId
            });

            return new PagedUserResponse(userResponses, totalItems, pageSize);
        }

        // =======================
        // ✅ NEW FEATURES
        // =======================

        public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                NationalId = user.NationalId
            };
        }

        public async Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            user.FullName = request.FullName ?? user.FullName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Address = request.Address ?? user.Address;
            user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
            user.Gender = request.Gender ?? user.Gender;

            await _db.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        // =======================
        // ✅ CHECK DUPLICATE FIELDS
        // =======================

        public async Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var query = _db.Users.Where(u => u.UserName == username);
            
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var query = _db.Users.Where(u => u.Email == email);
            
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> IsPhoneExistsAsync(string phone, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var query = _db.Users.Where(u => u.PhoneNumber == phone);
            
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return false;

            var query = _db.Users.Where(u => u.NationalId == nationalId);
            
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return await query.AnyAsync();
        }
    }
}
