using light_quiz_api.Dtos.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService, ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        [HttpPost("group/{shortCode}")]
        public async Task<IActionResult> NotifyGroup(string shortCode, [FromBody] PostGroupNotificationRequest request)
        {
            var groupId = await _context.Groups.FirstOrDefaultAsync(g => g.ShortCode == shortCode);

            if (groupId is null)
            {
                return NotFound("Group not found");
            }

            await _notificationService.NotifyGroupAsync(groupId.Id, request.NotificationTitle, request.NotificationBody);
            return Ok("Notification sent");
        }
    }
}
