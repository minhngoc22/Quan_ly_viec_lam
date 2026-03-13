using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoanWeb.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;



using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;



namespace DoanWeb.Controllers
{
    public class JobController : Controller
    {
        private readonly JobContext _context;
        public JobController(JobContext context)
        {
            _context = context;
        }

        // Tìm việc
        // Tìm việc
        public async Task<IActionResult> Index(string keyword, int? provinceId, int? categoryId, string? category)
        {
            // Lấy query base
            var jobsQuery = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Province)
                .AsQueryable();

            // Lọc sơ bộ theo keyword (giúp giảm dữ liệu trả về DB)
            if (!string.IsNullOrEmpty(keyword))
            {
                // lưu ý: dùng .ToLower() nếu DB collation phân biệt hoa thường
                jobsQuery = jobsQuery.Where(j =>
                    j.Title.Contains(keyword) ||
                    j.Description.Contains(keyword) ||
                    j.Keywords.Contains(keyword));
            }

            // Lọc theo tỉnh (nên làm trước khi ToListAsync)
            if (provinceId.HasValue)
                jobsQuery = jobsQuery.Where(j => j.ProvinceId == provinceId.Value);

            // Lọc theo CategoryId
            if (categoryId.HasValue)
                jobsQuery = jobsQuery.Where(j => j.CategoryId == categoryId.Value);

            // Lọc theo tên Category (nếu có)
            if (!string.IsNullOrEmpty(category))
                jobsQuery = jobsQuery.Where(j => j.Category.CategoryName.Contains(category));

            // Materialize kết quả (chỉ materialize 1 lần)
            var jobsList = await jobsQuery
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            // Nếu cần lọc nâng cao không dấu trên RAM
            if (!string.IsNullOrEmpty(keyword))
            {
                string keywordUnaccented = RemoveDiacritics(keyword.ToLower());
                jobsList = jobsList.Where(j =>
                    RemoveDiacritics((j.Title ?? "").ToLower()).Contains(keywordUnaccented) ||
                    RemoveDiacritics((j.Keywords ?? "").ToLower()).Contains(keywordUnaccented)
                ).ToList();
            }

            // Load dropdown
            var provinces = await _context.Provinces
                .OrderBy(p => p.ProvinceName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProvinceId.ToString(),
                    Text = p.ProvinceName
                }).ToListAsync();

            var categories = await _context.JobCategories
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToListAsync();

            var vm = new JobSearchViewModel
            {
                Jobs = jobsList,
                Provinces = provinces,
                Categories = categories,
                Keyword = keyword,
                ProvinceId = provinceId,
                CategoryId = categoryId
            };

            return View(vm);
        }



        public async Task<IActionResult> JobDetail(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Category)
                .Include(j => j.Province)
                .Include(j => j.Employer)
                    .ThenInclude(e => e.EmployerNavigation) // ⭐ load User từ Employer
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job == null) return NotFound();

            // Gợi ý việc tương tự
            var similarJobs = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Province)
                .Where(j => j.CategoryId == job.CategoryId && j.JobId != id)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.SimilarJobs = similarJobs;

            //load bình luận
            // Load bình luận
            var comments = _context.JobComments
                .Where(c => c.JobId == id)
                .Include(c => c.User)
                .Select(c => new JobCommentViewModel
                {
                    UserName = c.User.FullName,
                    Avatar = string.IsNullOrEmpty(c.User.Avatar) ? "/images/avatar/default.jpg" : c.User.Avatar,
                    AvatarColor = string.IsNullOrEmpty(c.User.AvatarColor) ? "#6c757d" : c.User.AvatarColor,
                    CommentText = c.CommentText,
                    CreatedAt = c.CreatedAt ?? DateTime.Now
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.Comments = comments;

            return View(job);
        }
        // Trang tìm việc + lọc
        public async Task<IActionResult> AllJobs(AllJobsViewModel model)
        {
            // 1️⃣ Lấy IQueryable từ DB
            var jobsQuery = _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Province)
                .Include(j => j.Category)
                .AsQueryable();

            // 2️⃣ Lọc theo từ khóa
            if (!string.IsNullOrEmpty(model.Keyword))
            {
                jobsQuery = jobsQuery.Where(j =>
                    j.Title.Contains(model.Keyword) ||
                    j.Description.Contains(model.Keyword) ||
                    j.Company.CompanyName.Contains(model.Keyword));
            }

            // 3️⃣ Lọc theo nhiều tỉnh
            if (model.SelectedProvinceIds?.Any() == true)
                jobsQuery = jobsQuery.Where(j => j.ProvinceId.HasValue && model.SelectedProvinceIds.Contains(j.ProvinceId.Value));

            // 4️⃣ Lọc theo loại công việc
            if (!string.IsNullOrEmpty(model.SelectedJobType))
                jobsQuery = jobsQuery.Where(j => j.JobType == model.SelectedJobType);

            // 5️⃣ Lọc theo danh mục công việc
            if (model.SelectedCategoryIds?.Any() == true)
                jobsQuery = jobsQuery.Where(j => j.CategoryId.HasValue && model.SelectedCategoryIds.Contains(j.CategoryId.Value));

            // 6️⃣ Tạo danh sách job trước
            List<Job> jobList = await jobsQuery
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            // 7️⃣ Lọc theo khoảng lương
            if (model.SelectedSalaryRangeId.HasValue)
            {
                var range = SalaryRange.GetSalaryRanges()
                    .FirstOrDefault(r => r.Id == model.SelectedSalaryRangeId.Value);

                if (range != null)
                {
                    jobList = jobList
                        .Where(j =>
                        {
                            var (minSalary, maxSalary) = SalaryHelper.ParseSalary(j.Salary);

                            // Bỏ qua job không có lương
                            if (!minSalary.HasValue && !maxSalary.HasValue)
                                return false;

                            // Giá trị mặc định nếu thiếu 1 đầu
                            minSalary ??= maxSalary ?? 0;
                            maxSalary ??= minSalary ?? 0;

                            // Logic: chỉ loại khi hoàn toàn ngoài khoảng
                            if (range.Min.HasValue && maxSalary < range.Min.Value)
                                return false;
                            if (range.Max.HasValue && minSalary > range.Max.Value)
                                return false;

                            return true; // Có giao nhau
                        })
                        .ToList();
                }
            }


            // 8️⃣ Gán kết quả vào model
            model.Jobs = jobList;

            // 9️⃣ Message nếu không có kết quả
            if (!model.Jobs.Any())
                ViewBag.Message = "Không tìm thấy công việc nào phù hợp.";

            // 10️⃣ Load dropdown cho filter
            model.Provinces = await _context.Provinces.OrderBy(p => p.ProvinceName).ToListAsync();
            model.Categories = await _context.JobCategories.OrderBy(c => c.CategoryName).ToListAsync();
            model.JobTypes = await _context.Jobs
                .Select(j => j.JobType)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToListAsync();
            model.SalaryRanges = SalaryRange.GetSalaryRanges();

            return View(model);
        }



        public async Task<IActionResult> Apply(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null || role != "Candidate")
            {
                ViewBag.Error = "Bạn cần đăng nhập bằng tài khoản ứng viên!";
                return View("JobDetail", await _context.Jobs.FindAsync(id));
            }

            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.CandidateId == userId);

            if (candidate == null)
            {
                ViewBag.Error = "Bạn chưa có hồ sơ ứng viên. Vui lòng cập nhật thông tin!";
                return View("JobDetail", await _context.Jobs.FindAsync(id));
            }

            var cv = await _context.Cvs.FirstOrDefaultAsync(c => c.CandidateId == candidate.CandidateId);
            if (cv == null)
            {
                ViewBag.Error = "Bạn chưa upload CV. Vui lòng bổ sung hồ sơ!";
                return View("JobDetail", await _context.Jobs.FindAsync(id));
            }

            bool exists = await _context.Applications
                .AnyAsync(a => a.JobId == id && a.CandidateId == candidate.CandidateId);

            if (exists)
            {
                ViewBag.Error = "Bạn đã ứng tuyển công việc này!";
                return View("JobDetail", await _context.Jobs.FindAsync(id));
            }

            var application = new Application
            {
                JobId = id,
                CandidateId = candidate.CandidateId,
                AppliedAt = DateTime.Now,
                Status = "Pending"
            };

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            // Gửi Notification cho Employer
            // Lấy job + employer + user (navigation)
            var job = await _context.Jobs
                .Include(j => j.Employer)
                    .ThenInclude(e => e.EmployerNavigation) // 👈 để lấy User
                .FirstOrDefaultAsync(j => j.JobId == id);

            if (job?.Employer?.EmployerNavigation != null)
            {
                var notification = new Notification
                {
                    UserId = job.Employer.EmployerNavigation.UserId, // 👈 UserId từ bảng Users
                    Message = $"Ứng viên mới vừa ứng tuyển vào công việc: {job.Title}",
                    Type = "Application",
                    Link = "/Employer/ManageCandidates/{job.JobId}",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }


            // ✅ Dùng ViewBag thay cho TempData
            ViewBag.Success = "Ứng tuyển thành công. CV của bạn đã được gửi đến nhà tuyển dụng!";
            return View("JobDetail", job);
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int jobId, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["CommentError"] = "Bạn cần đăng nhập để bình luận.";
                return RedirectToAction("JobDetail", new { id = jobId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "Nội dung bình luận không được để trống.";
                return RedirectToAction("JobDetail", new { id = jobId });
            }

            // 1️⃣ Lưu bình luận mới
            var comment = new JobComment
            {
                JobId = jobId,
                UserId = userId.Value,
                CommentText = content,
                CreatedAt = DateTime.Now
            };

            _context.JobComments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["CommentSuccess"] = "Bình luận đã được gửi!";

            // 2️⃣ Redirect sang JobDetail, action này sẽ load lại danh sách bình luận
            return RedirectToAction("JobDetail", new { id = jobId });
        }


        


    }






}
