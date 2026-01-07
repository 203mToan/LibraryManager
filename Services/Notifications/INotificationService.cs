using MyApi.Model.Response;

namespace MyApi.Services.Notifications
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotificationAsync(Guid userId, string title, string message, string type, int? loanId = null);
        Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId);
        Task<List<NotificationResponse>> GetUnreadNotificationsAsync(Guid userId);
        Task<NotificationResponse?> MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(int notificationId);
    }
}
