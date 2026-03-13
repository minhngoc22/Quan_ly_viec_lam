using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DoanWeb.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly JobContext _context;

        public NotificationViewComponent(JobContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return View("Default", new List<Notification>());

            var notifications = _context.Notifications
                .Where(n => n.UserId == userId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.UnreadCount = _context.Notifications
                .Count(n => n.UserId == userId.Value && !n.IsRead);

            return View("Default", notifications);
        }
    }
}
