using MyApi.Entities;

namespace MyApi.Services.Identity
{
    public interface IIdentityService
    {
        Guid? GetUserId();
        Task<User?> GetCurrentUserAsync();
        string? GetUserName();
        string? GetEmail();
        string? GetRole();
        bool IsAuthenticated();
    }
}
