using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoanWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DoanWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly JobContext _context;

        public HomeController(JobContext context)
        {
            _context = context;
        }

        // Trang chủ
        // Trang chủ
        // Trang chủ
        public async Task<IActionResult> Index()
        {
            // ✅ 1. Lấy 6 việc làm mới nhất đã được duyệt
            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Province)
                   .Include(j => j.Applications) // 🔹 Thêm include này
                .Where(j => j.IsApproved == true)

                .OrderByDescending(j => j.CreatedAt)
                .Take(6)
                .ToListAsync();

            // ✅ 2. Lấy danh sách tỉnh/thành
            var provinces = await _context.Provinces
                .OrderBy(p => p.ProvinceName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProvinceId.ToString(),
                    Text = p.ProvinceName
                }).ToListAsync();

            // ✅ 3. Lấy danh sách JobCategories chia 2 cột (giữ nguyên)
            var jobCategories = await _context.JobCategories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            int half = (int)System.Math.Ceiling(jobCategories.Count / 2.0);
            ViewBag.CategoriesCol1 = jobCategories.Take(half).ToList();
            ViewBag.CategoriesCol2 = jobCategories.Skip(half).ToList();

            // ✅ 4. Lấy công ty hàng đầu (có job đã duyệt)
            var topCompanies = await _context.Companies
                .Include(c => c.Jobs)
                .OrderByDescending(c => c.Jobs.Count(j => j.IsApproved == true))
                .Take(8)
                .ToListAsync();

            // ✅ 5. Lấy 5 ngành có nhiều việc làm nhất
            var topCategories = await _context.JobCategories
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    JobCount = _context.Jobs.Count(j => j.CategoryId == c.CategoryId && j.IsApproved == true)
                })
                .OrderByDescending(c => c.JobCount)
                .Take(5)
                .ToListAsync();

            ViewBag.TopCategories = topCategories;

            // ✅ 6. Tạo ViewModel chính
            var vm = new JobSearchViewModel
            {
                Jobs = jobs,
                Provinces = provinces,
                TopCompanies = topCompanies
            };

            // ✅ 7. Lưu danh sách job đã lưu của ứng viên (nếu có)
            int? candidateId = HttpContext.Session.GetInt32("UserId");
            List<int> savedJobIds = new List<int>();
            if (candidateId.HasValue)
            {
                savedJobIds = _context.SavedJobs
                    .Where(s => s.CandidateId == candidateId.Value)
                    .Select(s => s.JobId)
                    .ToList();
            }
            ViewBag.SavedJobIds = savedJobIds;

            // ✅ 8. Trả về View
            return View(vm);
        }



        // Chi tiết việc làm
        public async Task<IActionResult> JobDetail(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Province)
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null) return NotFound();

            return View(job);
        }

        public void LoadNotifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var notifications = _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .ToList();

                ViewBag.NotificationCount = notifications.Count(n => !n.IsRead);
                ViewBag.Notifications = notifications.Select(n => n.Message).ToList();
            }
            else
            {
                ViewBag.NotificationCount = 0;
                ViewBag.Notifications = new List<string>();
            }
        }

    }
}
