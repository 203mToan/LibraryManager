using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Services.Notifications;
using System.Security.Claims;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// L?y danh sách thông báo c?a user hi?n t?i
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// L?y danh sách thông báo ch?a ??c + s? l??ng
        /// </summary>
        [Authorize]
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _notificationService.GetUnreadNotificationsAsync(userId);
                return Ok(new 
                { 
                    UnreadCount = result.Count,
                    Notifications = result 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// ?ánh d?u thông báo là ?ã ??c
        /// </summary>
        [Authorize]
        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(id);
                if (result == null)
                    return NotFound(new { Message = "Notification not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// ?ánh d?u t?t c? thông báo là ?ã ??c
        /// </summary>
        [Authorize]
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { Message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa thông báo
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id);
                return Ok(new { Message = "Notification deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
