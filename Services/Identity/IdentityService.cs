using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using System.Security.Claims;

namespace MyApi.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _context;
        private readonly AppDbContext _db;

        public IdentityService(IHttpContextAccessor context, AppDbContext db)
        {
            _context = context;
            _db = db;
        }

        private ClaimsPrincipal? User => _context.HttpContext?.User;

        public bool IsAuthenticated()
            => User?.Identity?.IsAuthenticated ?? false;

        public Guid? GetUserId()
        {
            var id = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(id, out var guid))
                return guid;

            return null;
        }

        public string? GetUserName()
            => User?.FindFirstValue(ClaimTypes.Name);

        public string? GetEmail()
            => User?.FindFirstValue(ClaimTypes.Email);

        public string? GetRole()
            => User?.FindFirstValue(ClaimTypes.Role);

        public async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetUserId();
            if (userId == null) return null;

            return await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
