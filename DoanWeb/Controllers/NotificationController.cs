using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DoanWeb.Controllers
{
    public class NotificationController : Controller
    {
        private readonly JobContext _context;

        public NotificationController(JobContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new List<object>());

            var notifications = _context.Notifications
                .Where(n => n.UserId == userId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList() // 👈 kéo về memory trước
                .Select(n => new {
                    id = n.Id,
                    message = n.Message.Contains("Lý do:")
        ? n.Message.Split("Lý do:")[0].Trim()
        : n.Message, // chỉ lấy phần trước "Lý do"
                    createdAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    isRead = n.IsRead,
                    reason = n.Message.Contains("Lý do:")
        ? n.Message.Split("Lý do:")[1].Trim()
        : "" // lấy phần sau "Lý do" nếu có
                })

                .ToList();

            return Json(notifications);
        }





        [HttpPost]
        public IActionResult MarkAsRead([FromBody] MarkAsReadDto dto)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == dto.Id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
            }
            return Ok(new { success = true });
        }

        public class MarkAsReadDto
        {
            public int Id { get; set; }
        }


        [HttpPost]
        public IActionResult MarkAllAsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var unread = _context.Notifications
                    .Where(n => n.UserId == userId.Value && !n.IsRead)
                    .ToList();

                foreach (var n in unread)
                    n.IsRead = true;

                _context.SaveChanges();
            }
            return Ok(new { success = true });
        }
    }
}
