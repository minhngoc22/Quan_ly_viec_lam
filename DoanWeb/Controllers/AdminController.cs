using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using static DoanWeb.Models.AdminDashboardViewModel;

namespace DoanWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly JobContext _context;

        public AdminController(JobContext context)
        {
            _context = context;
        }

        // ============================
        // DASHBOARD ADMIN
        // ============================
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalEmployers = await _context.Users.Where(u => u.Role == "Employer").CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalJobsPending = await _context.Jobs.Where(j => j.IsApproved == false).CountAsync(), // ✅
                TotalReports = await _context.Reports.CountAsync(),

                // ✅ Đếm tổng cẩm nang
                TotalCareerGuides = await _context.CareerGuides.CountAsync(),

                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync(),

                PendingJobs = await _context.Jobs
                    .Where(j => j.IsApproved == false)
                    .Include(j => j.Employer)
                    .Include(j => j.Company)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .ToListAsync(),


                // ✅ Chỉ báo cáo chưa xử lý mới tính vào badge
                TotalReportsPending = _context.Reports.Count(r => r.Status == "Chưa xử lý"),
                RecentReports = await _context.Reports
                    .Include(r => r.Reporter)
                    .OrderByDescending(r => r.ReportId)
                    .Take(5)
                    .ToListAsync(),
                // 🔥 Lấy top 5 job có nhiều bình luận nhất
                // 🔥🔥🔥 THÊM THỐNG KÊ BÌNH LUẬN
                // 🔥🔥🔥 THÊM THỐNG KÊ BÌNH LUẬN
                TotalJobComments = await _context.JobComments.CountAsync(),


                TopCommentedJobs = await _context.Jobs
            .Select(j => new JobCommentStats
            {
                JobId = j.JobId,
                Title = j.Title,
                CommentCount = _context.JobComments.Count(c => c.JobId == j.JobId)
            })
            .OrderByDescending(x => x.CommentCount)
            .Take(5)
            .ToListAsync()


            };

            return View(model);
        }


        // ============================
        // QUẢN LÝ NGƯỜI DÙNG
        // ============================
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Status = !user.Status;
                await _context.SaveChangesAsync();

                _context.AdminActions.Add(new AdminAction
                {
                    AdminId = HttpContext.Session.GetInt32("UserId") ?? 0,
                    ActionType = (user.Status ?? false) ? "UnbanUser" : "BanUser",
                    TargetId = user.UserId,
                    Note = $"Thay đổi trạng thái người dùng {user.Email}"
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageUsers");
        }

        // ============================
        // DUYỆT VIỆC LÀM
        // ============================

        public async Task<IActionResult> JobManagement()
        {
            var pendingJobs = await _context.Jobs
                .Where(j => j.IsApproved != true)
                .Include(j => j.Company)
                .Include(j => j.Employer).ThenInclude(e => e.EmployerNavigation)
                .OrderByDescending(j => j.Priority)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();

            var approvedJobs = await _context.Jobs
                .Where(j => j.IsApproved == true)
                .Include(j => j.Company)
                .Include(j => j.Employer).ThenInclude(e => e.EmployerNavigation)
                .OrderBy(j => j.SortOrder)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();

            var model = new JobManagementViewModel
            {
                PendingJobs = pendingJobs,
                ApprovedJobs = approvedJobs
            };

            return View(model);
        }

        public async Task<IActionResult> ApproveJobs()
        {
            var jobs = await _context.Jobs
.Where(j => j.IsApproved != true) // tức là IsApproved = false hoặc null
                .Include(j => j.Company)
                .Include(j => j.Employer)              // Include bảng Employer
                    .ThenInclude(e => e.EmployerNavigation) // Include User bên trong Employer

  .OrderByDescending(j => j.Priority)   // 🔥 job hot đi đầu
        .ThenByDescending(j => j.CreatedAt)   // mới nhất lên trước
                .ToListAsync();

            return View(jobs);
        }

        [HttpPost]
        public async Task<IActionResult> MarkJobExpired(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            job.IsExpired = true; // cần thêm cột IsExpired bool
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateJobOrder([FromBody] List<JobPriorityUpdate> updates)
        {
            int order = 1;
            foreach (var u in updates)
            {
                var job = await _context.Jobs.FindAsync(u.JobId);
                if (job != null)
                {
                    job.SortOrder = order;
                    job.Priority = u.Priority;
                    order++;
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class JobPriorityUpdate
        {
            public int JobId { get; set; }
            public int Priority { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> ApproveJob(int id)
        {
            // 1️⃣ Load Job cùng Employer và User
            var job = await _context.Jobs
                .Include(j => j.Employer)
                    .ThenInclude(e => e.EmployerNavigation) // load User liên kết với Employer
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null)
            {
                // Không tìm thấy job
                TempData["ErrorMessage"] = "Không tìm thấy bài đăng.";
                return RedirectToAction("ApproveJobs");
            }

            // 2️⃣ Duyệt job
            job.IsApproved = true;
            await _context.SaveChangesAsync();

            // 3️⃣ Ghi log hành động Admin
            var adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId.HasValue)
            {
                _context.AdminActions.Add(new AdminAction
                {
                    AdminId = adminId.Value,
                    ActionType = "ApproveJob",
                    TargetId = job.JobId,
                    Note = $"Duyệt job: {job.Title ?? "(không có tiêu đề)"}",
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            // 4️⃣ Tạo notification cho người đăng bài (nếu có)
            var user = job.Employer?.EmployerNavigation;
            if (user != null)
            {
                var notification = new Notification
                {
                    UserId = user.UserId,
                    Message = $"Bài đăng '{job.Title ?? "(không có tiêu đề)"}' của bạn đã được admin duyệt.",
                    Type = "JobApproved",
                    Link = $"/Job/Details/{job.JobId}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Bài đăng '{job.Title ?? "(không có tiêu đề)"}' đã được duyệt thành công.";
            return RedirectToAction("ApproveJobs");
        }


        [HttpPost]
        public async Task<IActionResult> RejectJob(int id, string reason)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                // ✅ Xóa job
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                // ✅ Lấy adminId an toàn
                int adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

                // ✅ Ghi log hành động
                _context.AdminActions.Add(new AdminAction
                {
                    AdminId = adminId,
                    ActionType = "RejectJob",
                    TargetId = id,
                    Note = $"Từ chối job: {job.Title}, Lý do: {reason}"
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ApproveJobs"); // quay về trang duyệt job
        }

        public async Task<IActionResult> ApprovedJobs()
        {
            var jobs = await _context.Jobs
                .Where(j => j.IsApproved == true)
                .Include(j => j.Company)
                .Include(j => j.Employer)
                    .ThenInclude(e => e.EmployerNavigation)
                .OrderBy(j => j.SortOrder)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();

            return View(jobs);
        }


        // ============================
        // QUẢN LÝ BÁO CÁO
        // ============================
        public IActionResult ManageReports()
        {
            var reports = _context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            // ✅ Khi vào xem, chuyển "Chưa xử lý" -> "Đang xem"
            foreach (var report in reports)
            {
                if (report.Status == "Chưa xử lý")
                {
                    report.Status = "Đang xem";
                }
            }
            _context.SaveChanges();

            return View(reports);
        }


        [HttpPost]
        public async Task<IActionResult> ResolveReport(int id, string note)
        {
            var report = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.TargetUser)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report != null)
            {
                report.Status = "Đã xử lý";
                await _context.SaveChangesAsync();

                // 🔹 Gửi thông báo cho người bị báo cáo
                if (report.TargetUser != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = report.TargetUser.UserId,
                        Message = $"Báo cáo liên quan đến bạn đã được admin xử lý. Ghi chú: {note}",
                        Type = "ReportResult",
                        Link = "/Notification",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                // 🔹 Gửi thông báo cho người báo cáo
                if (report.Reporter != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = report.Reporter.UserId,
                        Message = $"Báo cáo của bạn đã được xử lý. Ghi chú: {note}",
                        Type = "ReportResult",
                        Link = "/Notification",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }

                // ✅ Lấy đúng UserId từ session
                var adminId = HttpContext.Session.GetInt32("UserId");

                if (adminId == null)
                {
                    // Nếu không có session thì chặn luôn (tránh lỗi FK)
                    return RedirectToAction("LoginAdmin", "Admin");
                }

                _context.AdminActions.Add(new AdminAction
                {
                    AdminId = adminId.Value,  // ✅ dùng Value
                    ActionType = "ResolveReport",
                    TargetId = report.ReportId,
                    Note = note,
                    CreatedAt = DateTime.Now
                });


                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageReports");
        }


        // ✅ Xóa báo cáo
        [HttpPost]
        public IActionResult DeleteReport(int id)
        {
            var report = _context.Reports.FirstOrDefault(r => r.ReportId == id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                _context.SaveChanges();
            }
            return RedirectToAction("ManageReports");
        }

        // ============================
        // QUẢN LÝ NGÀNH NGHỀ
        // ============================
        public async Task<IActionResult> ManageCategories()
        {
            var categories = await _context.JobCategories.ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _context.JobCategories.Add(new JobCategory { CategoryName = name });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageCategories");
        }

        //[HttpPost]
        //public async Task<IActionResult> DeleteCategory(int id)
        //{
        //    var category = await _context.JobCategories.FindAsync(id);
        //    if (category != null)
        //    {
        //        _context.JobCategories.Remove(category);
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction("ManageCategories");
        //}

        // ============================
        // LOG HÀNH ĐỘNG
        // ============================
        public async Task<IActionResult> AdminLogs()
        {
            var logs = await _context.AdminActions
                .Include(a => a.Admin)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(logs);
        }

        private bool IsSuperAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Admin"; // chỉ Admin chính mới có toàn quyền
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsSuperAdmin())
                return Forbid("Chỉ Admin chính mới được phép xóa danh mục.");

            var category = await _context.JobCategories.FindAsync(id);
            if (category != null)
            {
                _context.JobCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageCategories");
        }

        [HttpGet]
        public IActionResult AddAdmin()
        {
            if (!IsSuperAdmin())
                return Forbid();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(string fullname, string email, string password)
        {
            if (!IsSuperAdmin())
                return Forbid();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email đã tồn tại.";
                return View();
            }

            var newAdmin = new User
            {
                FullName = fullname,
                Email = email,
                PasswordHash = password, // (nên mã hóa sau)
                Role = "SubAdmin",
                Status = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newAdmin);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Tạo tài khoản admin phụ thành công!";
            return View();
        }

        // ============================
        // QUẢN LÝ CẨM NANG
        // ============================
        public async Task<IActionResult> ManageCareerGuides()
        {
            var guides = await _context.CareerGuides
                .OrderByDescending(g => g.PostedDate)
                .ToListAsync();
            return View(guides);
        }

        // GET: Thêm cẩm nang
        [HttpGet]
        public IActionResult AddCareerGuide()
        {
            return View();
        }

        // POST: Thêm cẩm nang (có upload ảnh)
        [HttpPost]
        public async Task<IActionResult> AddCareerGuide(
            string title,
            string description,
            string content,
            IFormFile image, // ⚡ Đổi từ string → IFormFile
            string author,
            string category)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                TempData["ErrorMessage"] = "Tiêu đề và nội dung không được để trống.";
                return RedirectToAction("AddCareerGuide");
            }

            string imagePath = null;

            // ✅ Xử lý upload ảnh nếu có
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/guides");

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file an toàn
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Ghi file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Lưu đường dẫn tương đối (để hiển thị sau)
                imagePath = "/images/guides/" + uniqueFileName;
            }

            // Tạo đối tượng cẩm nang
            var guide = new CareerGuide
            {
                Title = title,
                Description = description,
                Content = content,
                Image = imagePath, // lưu đường dẫn ảnh thực tế
                Author = author,
                Category = category,
                PostedDate = DateTime.Now,
                Views = 0,
                IsActive = true
            };

            _context.CareerGuides.Add(guide);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm cẩm nang thành công!";
            return RedirectToAction("ManageCareerGuides");
        }

        // GET: Sửa cẩm nang
        [HttpGet]
        public async Task<IActionResult> EditCareerGuide(int id)
        {
            var guide = await _context.CareerGuides.FindAsync(id);
            if (guide == null)
                return NotFound();
            return View(guide);
        }

        // POST: Sửa cẩm nang (hỗ trợ đổi ảnh)
        [HttpPost]
        public async Task<IActionResult> EditCareerGuide(
            int id,
            string title,
            string description,
            string content,
            IFormFile? image, // ⚡ Cho phép null
            string author,
            string category,
            bool isActive)
        {
            var guide = await _context.CareerGuides.FindAsync(id);
            if (guide == null)
                return NotFound();

            // Nếu người dùng upload ảnh mới → lưu lại
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/guides");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Xóa ảnh cũ (nếu có)
                if (!string.IsNullOrEmpty(guide.Image))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", guide.Image.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                guide.Image = "/images/guides/" + uniqueFileName;
            }

            guide.Title = title;
            guide.Description = description;
            guide.Content = content;
            guide.Author = author;
            guide.Category = category;
            guide.IsActive = isActive;
            guide.PostedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật cẩm nang thành công!";
            return RedirectToAction("ManageCareerGuides");
        }

        // POST: Xóa cẩm nang
        [HttpPost]
        public async Task<IActionResult> DeleteCareerGuide(int id)
        {
            var guide = await _context.CareerGuides.FindAsync(id);
            if (guide != null)
            {
                _context.CareerGuides.Remove(guide);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa cẩm nang thành công!";
            }
            return RedirectToAction("ManageCareerGuides");
        }

        // POST: Ẩn/Hiện cẩm nang
        [HttpPost]
        public async Task<IActionResult> ToggleCareerGuideVisibility(int id)
        {
            var guide = await _context.CareerGuides.FindAsync(id);
            if (guide != null)
            {
                guide.IsActive = !guide.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageCareerGuides");
        }


        [HttpPost]
        public IActionResult EditCategory(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Tên danh mục không hợp lệ.");

            var category = _context.JobCategories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
                return NotFound();

            category.CategoryName = name;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult SetUserPassword(int id, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return BadRequest("Mật khẩu không hợp lệ.");

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // ✅ Mã hóa SHA256 trước khi lưu
            user.PasswordHash = ComputeSha256Hash(password);

            _context.SaveChanges();
            return Ok("Đặt lại mật khẩu thành công!");
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }



    }
}
