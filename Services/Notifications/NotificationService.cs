using Microsoft.EntityFrameworkCore;
using MyApi.Entities;
using MyApi.Model.Response;

namespace MyApi.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<NotificationResponse> CreateNotificationAsync(Guid userId, string title, string message, string type, int? loanId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                LoanId = loanId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();

            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                LoanId = notification.LoanId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<List<NotificationResponse>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _db.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return notifications.Select(x => new NotificationResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                LoanId = x.LoanId,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<List<NotificationResponse>> GetUnreadNotificationsAsync(Guid userId)
        {
            var notifications = await _db.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return notifications.Select(x => new NotificationResponse
            {
                Id = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                LoanId = x.LoanId,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        public async Task<NotificationResponse?> MarkAsReadAsync(int notificationId)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId);
            if (notification == null) return null;

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                LoanId = notification.LoanId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _db.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId);
            if (notification != null)
            {
                _db.Notifications.Remove(notification);
                await _db.SaveChangesAsync();
            }
        }
    }
}
