using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace DoanWeb.Controllers
{
    public class EmployerController : Controller
    {
        private readonly JobContext _context;
        public EmployerController(JobContext context) => _context = context;

        // ==============================
        // DASHBOARD
        // ==============================
        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // chưa login → quay về login
                return RedirectToAction("LoginEmployer", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);
            ViewBag.User = user; // để hiển thị tên trong layout hoặc view
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View();
        }


        // ==============================
        // QUẢN LÝ VIỆC LÀM
        // ==============================

        // 🔹 Danh sách việc làm của employer
        public IActionResult ManageJobs()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!userId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            var jobs = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Company)
                .Where(j => j.EmployerId == userId.Value)
                .ToList();

            return View(jobs);
        }

        [HttpGet]
        public IActionResult CreateJob()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!userId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            var vm = new JobFormViewModel
            {
                Job = new Job(),
                Categories = _context.JobCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName
                    }).ToList(),
                Companies = _context.Companies
                    .Where(c => c.EmployerId == userId.Value)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CompanyId.ToString(),
                        Text = c.CompanyName
                    }).ToList(),
                Provinces = _context.Provinces
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProvinceId.ToString(),
                        Text = p.ProvinceName
                    }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJob(JobFormViewModel vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            // Kiểm tra quyền
            if (!userId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            // Xử lý validation cho navigation properties
            ModelState.Remove("Job.Company");
            ModelState.Remove("Job.Employer");

            // Validate cơ bản
            if (vm.Job.CompanyId == 0)
                ModelState.AddModelError("Job.CompanyId", "Vui lòng chọn công ty.");
            if (string.IsNullOrWhiteSpace(vm.Job.Title))
                ModelState.AddModelError("Job.Title", "Vui lòng nhập tiêu đề công việc.");
            if (string.IsNullOrWhiteSpace(vm.Job.Salary))
                ModelState.AddModelError("Job.Salary", "Vui lòng nhập mức lương.");
            if (vm.Job.ExpireDate == null)
                ModelState.AddModelError("Job.ExpireDate", "Vui lòng chọn ngày hết hạn.");

            if (ModelState.IsValid)
            {
                try
                {
                    // ✅ Xử lý upload ảnh nếu có
                    if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(vm.ImageFile.FileName)}";
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image/job");
                        if (!Directory.Exists(uploadPath))
                            Directory.CreateDirectory(uploadPath);

                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await vm.ImageFile.CopyToAsync(stream);
                        }

                        vm.Job.ImageUrl = "/image/job/" + fileName;
                    }

                    // Gán thông tin bắt buộc
                    vm.Job.EmployerId = userId.Value;
                    vm.Job.CreatedAt = DateTime.Now;
                    vm.Job.IsApproved = false;
                    vm.Job.GenderRequirement ??= "Không yêu cầu";
                    vm.Job.VacancyCount = vm.Job.VacancyCount == 0 ? 1 : vm.Job.VacancyCount;

                    // Thêm job vào DB
                    _context.Jobs.Add(vm.Job);
                    await _context.SaveChangesAsync();

                    // Gửi thông báo đến admin
                    var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
                    foreach (var admin in admins)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = admin.UserId,
                            Message = $"📢 Tin tuyển dụng mới: '{vm.Job.Title}' đã được đăng. Vui lòng duyệt.",
                            Type = "JobPendingApproval",
                            CreatedAt = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "🎉 Đăng tin thành công! Tin sẽ được admin duyệt sớm.";
                    return RedirectToAction("ManageJobs", "Employer");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Lỗi khi tạo job: {ex.Message}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng tin. Vui lòng thử lại.";
                }
            }

            // Nếu ModelState có lỗi → nạp lại dropdown
            vm.Categories = await _context.JobCategories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToListAsync();

            vm.Companies = await _context.Companies
                .Where(c => c.EmployerId == userId.Value)
                .Select(c => new SelectListItem
                {
                    Value = c.CompanyId.ToString(),
                    Text = c.CompanyName
                }).ToListAsync();

            vm.Provinces = await _context.Provinces
                .Select(p => new SelectListItem
                {
                    Value = p.ProvinceId.ToString(),
                    Text = p.ProvinceName
                }).ToListAsync();

            return View(vm);
        }



        // ==============================
        // CHỈNH SỬA JOB
        // ==============================

        // 🔹 GET: Employer/EditJob/5
        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!userId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            var job = await _context.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == userId.Value);

            if (job == null)
                return NotFound();

            var vm = new JobFormViewModel
            {
                Job = job,
                Categories = await _context.JobCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName
                    }).ToListAsync(),
                Companies = await _context.Companies
                    .Where(c => c.EmployerId == userId.Value)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CompanyId.ToString(),
                        Text = c.CompanyName
                    }).ToListAsync(),
                Provinces = await _context.Provinces
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProvinceId.ToString(),
                        Text = p.ProvinceName
                    }).ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(int id, JobFormViewModel vm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!userId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            if (id != vm.Job.JobId)
                return NotFound();

            ModelState.Remove("Job.Company");
            ModelState.Remove("Job.Employer");

            if (!ModelState.IsValid)
            {
                // reload dropdowns
                vm.Categories = await _context.JobCategories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToListAsync();
                vm.Companies = await _context.Companies.Where(c => c.EmployerId == userId.Value).Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName }).ToListAsync();
                vm.Provinces = await _context.Provinces.Select(p => new SelectListItem { Value = p.ProvinceId.ToString(), Text = p.ProvinceName }).ToListAsync();
                return View(vm);
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobId == id && j.EmployerId == userId.Value);
            if (job == null) return NotFound();

            // Update các trường
            job.Title = vm.Job.Title;
            job.Description = vm.Job.Description;
            job.Requirements = vm.Job.Requirements;
            job.Benefits = vm.Job.Benefits;
            job.Location = vm.Job.Location;
            job.Salary = vm.Job.Salary;
            job.JobType = vm.Job.JobType;
            job.GenderRequirement = vm.Job.GenderRequirement;
            job.VacancyCount = vm.Job.VacancyCount;
            job.ExpireDate = vm.Job.ExpireDate;
            job.CategoryId = vm.Job.CategoryId;
            job.ProvinceId = vm.Job.ProvinceId;
            job.CompanyId = vm.Job.CompanyId;
            job.Keywords = vm.Job.Keywords;
            job.IsApproved = false;

            // ✅ Xử lý ảnh
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(vm.ImageFile.FileName)}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/job");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.ImageFile.CopyToAsync(stream);

                job.ImageUrl = "/images/job/" + fileName;
            }

            _context.Update(job);
            await _context.SaveChangesAsync();

            // 🔔 Gửi thông báo cho Admin
            var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = admin.UserId,
                    Message = $"📢 Tin tuyển dụng '{job.Title}' đã được chỉnh sửa. Vui lòng duyệt lại.",
                    Type = "JobPendingApproval",
                    CreatedAt = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Cập nhật tin thành công! Vui lòng chờ admin duyệt lại.";
            return RedirectToAction("ManageJobs");
        }


        // 🔹 Xóa job
        public IActionResult DeleteJob(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");
            if (!userId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            var job = _context.Jobs.FirstOrDefault(j => j.JobId == id && j.EmployerId == userId.Value);
            if (job == null) return NotFound();

            _context.Jobs.Remove(job);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Xóa tin thành công!";
            return RedirectToAction("ManageJobs");
        }

        // ==============================
        // HỒ SƠ EMPLOYER
        // ==============================

        [HttpGet]
        public IActionResult Profile()
        {
            var employerId = HttpContext.Session.GetInt32("UserId");
            if (employerId == null)
                return RedirectToAction("LoginEmployer", "Account");

            // Lấy thông tin cá nhân
            var personal = (from e in _context.Employers
                            join u in _context.Users on e.EmployerId equals u.UserId
                            where e.EmployerId == employerId
                            select new EmployerPersonalViewModel
                            {
                                EmployerId = e.EmployerId,
                                FullName = u.FullName,
                                Email = u.Email,
                                Phone = e.Phone,
                                Position = e.Position
                            }).FirstOrDefault();

            // Lấy thông tin công ty
            var company = _context.Companies
                .Where(c => c.EmployerId == employerId)
                .Select(c => new CompanyViewModel
                {
                    CompanyID = c.CompanyId,
                    CompanyName = c.CompanyName,
                    Description = c.Description,
                    Address = c.Address,
                    Website = c.Website
                }).FirstOrDefault();

            if (personal == null || company == null)
                return NotFound();

            var model = new EmployerProfileViewModel
            {
                Personal = personal,
                Company = company
            };

            return View(model);
        }


        // 🔹 GET: Sửa thông tin cá nhân
        [HttpGet]
        public IActionResult EditPersonal()
        {
            var employerId = HttpContext.Session.GetInt32("UserId");
            if (employerId == null)
                return RedirectToAction("LoginEmployer", "Account");

            var employer = (from e in _context.Employers
                            join u in _context.Users on e.EmployerId equals u.UserId
                            where e.EmployerId == employerId
                            select new EmployerPersonalViewModel
                            {
                                EmployerId = e.EmployerId,
                                FullName = u.FullName,
                                Email = u.Email,
                                Phone = e.Phone,
                                Position = e.Position
                            }).FirstOrDefault();

            return employer == null ? NotFound() : View(employer);
        }

        // 🔹 POST: Sửa thông tin cá nhân & mật khẩu
        [HttpPost]
        public IActionResult EditPersonal(EmployerPersonalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "❌ Dữ liệu không hợp lệ, vui lòng kiểm tra lại.";
                return View(model);
            }

            try
            {
                var employer = _context.Employers.FirstOrDefault(e => e.EmployerId == model.EmployerId);
                var user = _context.Users.FirstOrDefault(u => u.UserId == model.EmployerId);

                if (employer == null || user == null)
                {
                    TempData["Error"] = "❌ Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Profile");
                }

                // Cập nhật thông tin cá nhân
                user.FullName = model.FullName;
                user.Email = model.Email;
                employer.Phone = model.Phone;
                employer.Position = model.Position;

                // Đổi mật khẩu nếu có nhập
                if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.NewPassword))
                {
                    bool passwordOk = false;

                    if (IsHashed(user.PasswordHash))
                        passwordOk = VerifyHashedPassword(user.PasswordHash, model.Password);
                    else
                        passwordOk = user.PasswordHash == model.Password;

                    if (!passwordOk)
                    {
                        // Thêm lỗi trực tiếp vào ModelState, sẽ hiển thị ngay dưới input
                        ModelState.AddModelError("Password", "❌ Mật khẩu cũ không chính xác!");
                        return View(model);
                    }

                    // Lưu mật khẩu mới luôn dạng hash
                    user.PasswordHash = HashPassword(model.NewPassword);
                }

                _context.SaveChanges();

                TempData["Success"] = "✅ Cập nhật thông tin cá nhân thành công!";
            }
            catch
            {
                TempData["Error"] = "❌ Có lỗi xảy ra khi cập nhật. Vui lòng thử lại!";
            }

            return View(model);
        }


        // ==============================
        // GET: Sửa thông tin công ty
        // ==============================
        [HttpGet]
        public IActionResult EditCompany()
        {
            var employerId = HttpContext.Session.GetInt32("UserId");
            if (employerId == null) return RedirectToAction("LoginEmployer", "Account");

            var company = _context.Companies
                .Where(c => c.EmployerId == employerId)
                .Select(c => new CompanyViewModel
                {
                    CompanyID = c.CompanyId,
                    CompanyName = c.CompanyName,
                    Description = c.Description,
                    Address = c.Address,
                    Website = c.Website,
                    Logo = c.Logo,
                }).FirstOrDefault();

            return company == null ? NotFound() : View(company);
        }

        // ==============================
        // POST: Sửa thông tin công ty
        // ==============================
        [HttpPost]
        public async Task<IActionResult> EditCompany(CompanyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var company = await _context.Companies.FindAsync(model.CompanyID);
                if (company == null) return NotFound();

                company.CompanyName = model.CompanyName;
                company.Address = model.Address;
                company.Website = model.Website;
                company.Description = model.Description;

                // ✅ Upload logo
                if (model.LogoFile != null && model.LogoFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.LogoFile.FileName)}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/image/logo");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.LogoFile.CopyToAsync(stream);
                    }

                    company.Logo = "/image/logo/" + fileName;
                }

                _context.Update(company);
               await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Cập nhật thành công!";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "❌ Upload logo bị lỗi: " + ex.Message;
                return View(model);
            }
        }




        // ==============================
        // HÀM HỖ TRỢ MẬT KHẨU
        // ==============================

        // Băm mật khẩu SHA256
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Kiểm tra password đã hash chưa (heuristic)
        private bool IsHashed(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            try
            {
                Convert.FromBase64String(password);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Verify password hash với input
        private bool VerifyHashedPassword(string hashedPassword, string password)
        {
            return hashedPassword == HashPassword(password);
        }

        // ==============================
        // QUẢN LÝ ỨNG VIÊN
        // ==============================


        // 🔹 Xem danh sách ứng viên theo Job
        // 🔹 Xem danh sách ứng viên ứng tuyển theo Job
        // 🔹 Xem danh sách ứng viên đã ứng tuyển các job của employer
        public IActionResult ManageCandidates()
        {
            var employerId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!employerId.HasValue || role != "Employer")
                return RedirectToAction("LoginEmployer", "Account");

            // Lấy danh sách các job của employer
            var jobIds = _context.Jobs
                .Where(j => j.EmployerId == employerId.Value)
                .Select(j => j.JobId)
                .ToList();

            //// Lấy danh sách ứng viên đã ứng tuyển
            //var applications = _context.Applications
            //    .Include(a => a.Candidate)
            //        .ThenInclude(c => c.CandidateNavigation) // lấy FullName & Email
            //    .Include(a => a.Job)
            //    .Where(a => jobIds.Contains(a.JobId))
            //    .OrderByDescending(a => a.AppliedAt)
            //    .ToList();

            // Lấy danh sách ứng viên đã ứng tuyển, kèm CV
            var applications = _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.CandidateNavigation) // lấy FullName & Email
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.Cvs) // join CV
                .Where(a => jobIds.Contains(a.JobId))
                .OrderByDescending(a => a.AppliedAt)
                .ToList();

            return View(applications);
        }

        [HttpPost]
        public IActionResult UpdateCandidateStatus(int applicationId, string status, string? rejectReason)
        {
            var application = _context.Applications
                .Include(a => a.Candidate)
                    .ThenInclude(c => c.CandidateNavigation)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .FirstOrDefault(a => a.ApplicationId == applicationId);

            if (application != null)
            {
                // Nếu đã từ chối rồi thì không cho update
                if (application.Status == "Từ chối")
                {
                    TempData["Error"] = "Ứng viên này đã bị từ chối, không thể cập nhật trạng thái nữa.";
                    return RedirectToAction("ManageCandidates");
                }

                application.Status = status;

                // 👇 lưu lý do từ chối trực tiếp vào Application
                if (status == "Từ chối")
                {
                    application.RejectReason = rejectReason;
                }

                _context.SaveChanges();

                // Gửi thông báo cho ứng viên
                string message = status switch
                {
                    "Chấp nhận" => $"🎉 Đơn ứng tuyển của bạn vào vị trí '{application.Job?.Title}' đã được duyệt!",
                    "Từ chối" => $"❌ Đơn ứng tuyển của bạn vào vị trí '{application.Job?.Title}' đã bị từ chối. Lý do: {rejectReason}",
                    _ => $"📢 Đơn ứng tuyển của bạn vào vị trí '{application.Job?.Title}' đã được cập nhật trạng thái: {status}"
                };

                var notification = new Notification
                {
                    UserId = application.Candidate.CandidateNavigation.UserId,
                    Message = message,
                    Type = "Application",
                    Link = $"/Candidate/Applications/{application.ApplicationId}", // 👈 dẫn thẳng vào chi tiết đơn ứng tuyển
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("ManageCandidates");
        }


        // Tải form mẫu Word
        // Tải form mẫu Word (chỉ nhãn, không có dữ liệu mẫu)
        public IActionResult DownloadJobTemplate()
        {
            using var ms = new MemoryStream();

            using (var wordDoc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Tiêu đề chính
                var titlePara = new Paragraph(
                    new Run(
                        new Text("Đăng tin tuyển dụng") { Space = SpaceProcessingModeValues.Preserve }
                    )
                );
                titlePara.ParagraphProperties = new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center }
                );
                titlePara.Descendants<Run>().First().RunProperties = new RunProperties(new Bold());
                body.AppendChild(titlePara);

                body.AppendChild(new Paragraph(new Run(new Text(" ")))); // dòng trống

                // Các nhãn form (chỉ có label)
                string[] labels = new string[]
                {
            "Tiêu đề:",
            "Mức lương:",
            "Mô tả công việc:",
            "Yêu cầu công việc:",
            "Quyền lợi:",
            "Địa điểm làm việc:",
            "Giới tính:",
            "Số lượng:",
            "Hạn nộp:",
            "Từ khóa tìm kiếm:",
            "Ảnh mô tả:"
                };

                foreach (var label in labels)
                {
                    // Label
                    var paraLabel = new Paragraph(new Run(new Text(label) { Space = SpaceProcessingModeValues.Preserve }));
                    paraLabel.ParagraphProperties = new ParagraphProperties(
                        new SpacingBetweenLines() { After = "200" }
                    );
                    paraLabel.Descendants<Run>().First().RunProperties = new RunProperties(new Bold());
                    body.AppendChild(paraLabel);

                    // Dòng trống để điền thông tin
                    body.AppendChild(new Paragraph(new Run(new Text(" "))));
                }
            }

            ms.Seek(0, SeekOrigin.Begin);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "JobFormTemplate.docx");
        }



        [HttpPost]
        public async Task<IActionResult> ImportWord(IFormFile wordFile)
        {
            if (wordFile == null || wordFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Word.";
                return RedirectToAction("CreateJob", "Employer");
            }

            var jobs = new List<Job>();

            using (var stream = new MemoryStream())
            {
                await wordFile.CopyToAsync(stream);
                stream.Position = 0;

                using (var wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    var paragraphs = wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>().ToList();

                    Job currentJob = null;

                    foreach (var para in paragraphs)
                    {
                        // Lấy text và trim
                        var text = string.Join("", para.Elements<Run>().Select(r => r.InnerText)).Trim();
                        if (string.IsNullOrEmpty(text)) continue;

                        // So sánh nhãn ignore case
                        string lowerText = text.ToLower();

                        if (lowerText.StartsWith("tiêu đề:"))
                        {
                            if (currentJob != null && !string.IsNullOrEmpty(currentJob.Title))
                                jobs.Add(currentJob);

                            currentJob = new Job
                            {
                                Title = text.Substring("Tiêu đề:".Length).Trim()
                            };
                        }
                        else if (currentJob != null)
                        {
                            if (lowerText.StartsWith("mức lương:"))
                                currentJob.Salary = text.Substring("Mức lương:".Length).Trim();
                            else if (lowerText.StartsWith("mô tả công việc:"))
                                currentJob.Description = text.Substring("Mô tả công việc:".Length).Trim();
                            else if (lowerText.StartsWith("yêu cầu công việc:"))
                                currentJob.Requirements = text.Substring("Yêu cầu công việc:".Length).Trim();
                            else if (lowerText.StartsWith("quyền lợi:"))
                                currentJob.Benefits = text.Substring("Quyền lợi:".Length).Trim();
                            else if (lowerText.StartsWith("địa điểm làm việc:"))
                                currentJob.Location = text.Substring("Địa điểm làm việc:".Length).Trim();
                            else if (lowerText.StartsWith("giới tính:"))
                                currentJob.GenderRequirement = text.Substring("Giới tính:".Length).Trim();
                            else if (lowerText.StartsWith("số lượng:"))
                                currentJob.VacancyCount = int.TryParse(text.Substring("Số lượng:".Length).Trim(), out var count) ? count : 1;
                            else if (lowerText.StartsWith("hạn nộp:"))
                                currentJob.ExpireDate = DateTime.TryParse(text.Substring("Hạn nộp:".Length).Trim(), out var dt) ? dt : (DateTime?)null;
                            else if (lowerText.StartsWith("từ khóa tìm kiếm:"))
                                currentJob.Keywords = text.Substring("Từ khóa tìm kiếm:".Length).Trim();
                            else if (lowerText.StartsWith("ảnh mô tả:"))
                                currentJob.ImageUrl = text.Substring("Ảnh mô tả:".Length).Trim();
                        }
                    }

                    // Thêm job cuối cùng
                    if (currentJob != null && !string.IsNullOrEmpty(currentJob.Title))
                        jobs.Add(currentJob);
                }
            }

            if (!jobs.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy job hợp lệ trong file.";
            }
            // Lấy userId từ session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Không xác định được User. Vui lòng đăng nhập lại.";
                return RedirectToAction("LoginEmployer", "Account");
            }

            // Load dropdown cho form
            var vm = new JobFormViewModel
            {
                Job = jobs.FirstOrDefault() ?? new Job(),
                ImportedJobs = jobs,
                Provinces = await _context.Provinces.OrderBy(p => p.ProvinceName)
                    .Select(p => new SelectListItem { Value = p.ProvinceId.ToString(), Text = p.ProvinceName }).ToListAsync(),
                Categories = await _context.JobCategories.OrderBy(c => c.CategoryName)
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToListAsync(),
                Companies = await _context.Companies
                    .Where(c => c.EmployerId == userId.Value) // 🔹 chỉ lấy công ty của employer hiện tại
                    .OrderBy(c => c.CompanyName)
                    .Select(c => new SelectListItem { Value = c.CompanyId.ToString(), Text = c.CompanyName })
                    .ToListAsync()
            };

            return View("~/Views/Employer/CreateJob.cshtml", vm);

        }



    }

}
