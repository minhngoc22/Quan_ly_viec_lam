using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DoanWeb.Controllers
{
    public class CandidateController : Controller
    {
        private readonly JobContext _context;

        public CandidateController(JobContext context)
        {
            _context = context;
        }

        // ===========================
        // DASHBOARD ỨNG VIÊN
        // ===========================
        public IActionResult Dashboard()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.UserId == candidateId.Value);
            var candidate = _context.Candidates.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (user == null || candidate == null) return NotFound();

            var cv = _context.Cvs.FirstOrDefault(c => c.CandidateId == candidateId.Value);

            var applications = _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Where(a => a.CandidateId == candidateId.Value)
                .Select(a => new ApplicationItem
                {
                    JobTitle = a.Job.Title,
                    CompanyName = a.Job.Company.CompanyName,
                    Status = a.Status
                }).ToList();

            var savedJobs = _context.SavedJobs
                .Include(s => s.Job)
                    .ThenInclude(j => j.Company)
                .Where(s => s.CandidateId == candidateId.Value)
                .Select(s => new SavedJobItem
                {
                    JobTitle = s.Job.Title,
                    CompanyName = s.Job.Company.CompanyName
                }).ToList();

            var model = new CandidateDashboardViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = candidate.Phone,
                Address = candidate.Address,
                DateOfBirth = candidate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                Gender = candidate.Gender,
                CareerGoal = cv?.CareerGoal,
                CVFile = cv?.FilePath,
                Applications = applications,
                SavedJobs = savedJobs
            };

            return View(model);
        }

        // ===========================
        // HỒ SƠ ỨNG VIÊN
        // ===========================
        [HttpGet]
        public IActionResult Profile()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            var user = _context.Users.FirstOrDefault(u => u.UserId == candidateId.Value);
            var candidate = _context.Candidates.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            var cv = _context.Cvs.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (user == null || candidate == null) return NotFound();

            var model = new CandidateProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = candidate.Phone,
                Address = candidate.Address,
                DateOfBirth = candidate.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                Gender = candidate.Gender,
                CareerGoal = cv?.CareerGoal,
                CVFile = cv?.FilePath,
                Avatar = string.IsNullOrEmpty(user.Avatar) ? "/images/avatar/default.jpg" : user.Avatar,
                AvatarColor = string.IsNullOrEmpty(user.AvatarColor) ? "#6c757d" : user.AvatarColor
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(CandidateProfileViewModel model)
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            var user = _context.Users.FirstOrDefault(u => u.UserId == candidateId.Value);
            var candidate = _context.Candidates.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (user == null || candidate == null) return NotFound();

            // Cập nhật thông tin
            user.FullName = model.FullName;
            candidate.Phone = model.Phone;
            candidate.Address = model.Address;
            candidate.DateOfBirth = model.DateOfBirth.HasValue ? DateOnly.FromDateTime(model.DateOfBirth.Value) : null;
            candidate.Gender = model.Gender;

            // 🔹 Upload avatar
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatar");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.AvatarFile.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn avatar vào CSDL
                user.Avatar = $"/images/avatar/{fileName}";
            }

            _context.Users.Update(user);
            _context.Candidates.Update(candidate);
            await _context.SaveChangesAsync(); // nhớ dùng await

            // Load lại avatar xuống model
            model.Avatar = string.IsNullOrEmpty(user.Avatar) ? "/images/avatar/default.jpg" : user.Avatar;
            model.AvatarColor = string.IsNullOrEmpty(user.AvatarColor) ? "#6c757d" : user.AvatarColor;

            ViewBag.SuccessMessage = "Cập nhật hồ sơ thành công!";
            return View(model);
        }


        // ===========================
        // CV ỨNG VIÊN (THEO MẪU MỚI)
        // ===========================

        public IActionResult CV()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            var cv = _context.Cvs.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (cv == null)
                return RedirectToAction("CreateCV");

            // Lấy thông tin user (candidate)
            var user = _context.Users.FirstOrDefault(u => u.UserId == candidateId.Value);

            var vm = new CvViewModel
            {
                Cv = cv,
                Skills = _context.CvSkills.Where(s => s.Cvid == cv.Cvid).ToList(),
                Experiences = _context.CvExperiences.Where(e => e.Cvid == cv.Cvid).ToList(),
                Educations = _context.CvEducations.Where(ed => ed.Cvid == cv.Cvid).ToList(),
                Certificates = _context.CvCertificates.Where(cer => cer.Cvid == cv.Cvid).ToList(),

                // Lấy avatar từ user
                Avatar = user != null && !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : "/images/avatar/default.jpg",
                AvatarColor = user != null && !string.IsNullOrEmpty(user.AvatarColor) ? user.AvatarColor : "#6c757d"
            };

            return View(vm);
        }


        [HttpGet]
        public IActionResult CreateCV()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            return View(new CvViewModel());
        }

        [HttpPost]
        public IActionResult CreateCV(CvViewModel model, IFormFile? file)
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            if (!ModelState.IsValid) return View(model);

            // Upload file PDF nếu có
            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/cvs", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                model.Cv.FilePath = "/cvs/" + fileName;
            }

            model.Cv.CandidateId = candidateId.Value;
            _context.Cvs.Add(model.Cv);
            _context.SaveChanges();

            var cvId = model.Cv.Cvid;

            // Thêm các bảng con
            if (model.Skills.Any())
            {
                model.Skills.ForEach(s => s.Cvid = cvId);
                _context.CvSkills.AddRange(model.Skills);
            }

            if (model.Experiences.Any())
            {
                model.Experiences.ForEach(e => e.Cvid = cvId);
                _context.CvExperiences.AddRange(model.Experiences);
            }

            if (model.Educations.Any())
            {
                model.Educations.ForEach(ed => ed.Cvid = cvId);
                _context.CvEducations.AddRange(model.Educations);
            }

            if (model.Certificates.Any())
            {
                model.Certificates.ForEach(cer => cer.Cvid = cvId);
                _context.CvCertificates.AddRange(model.Certificates);
            }

            _context.SaveChanges();

            TempData["Success"] = "Tạo CV thành công!";
            return RedirectToAction("CV");
        }

        [HttpGet]
        public IActionResult EditCV()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            var cv = _context.Cvs.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (cv == null) return RedirectToAction("CreateCV");

            var vm = new CvViewModel
            {
                Cv = cv,
                Skills = _context.CvSkills.Where(s => s.Cvid == cv.Cvid).ToList(),
                Experiences = _context.CvExperiences.Where(e => e.Cvid == cv.Cvid).ToList(),
                Educations = _context.CvEducations.Where(ed => ed.Cvid == cv.Cvid).ToList(),
                Certificates = _context.CvCertificates.Where(cer => cer.Cvid == cv.Cvid).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult EditCV(CvViewModel model, IFormFile? file)
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            var cv = _context.Cvs.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (cv == null) return RedirectToAction("CreateCV");

            // Cập nhật thông tin chính
            cv.CareerGoal = model.Cv.CareerGoal;
            cv.Avatar = model.Cv.Avatar;

            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/cvs", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                cv.FilePath = "/cvs/" + fileName;
            }

            _context.Cvs.Update(cv);

            // Xóa dữ liệu cũ
            _context.CvSkills.RemoveRange(_context.CvSkills.Where(s => s.Cvid == cv.Cvid));
            _context.CvExperiences.RemoveRange(_context.CvExperiences.Where(e => e.Cvid == cv.Cvid));
            _context.CvEducations.RemoveRange(_context.CvEducations.Where(ed => ed.Cvid == cv.Cvid));
            _context.CvCertificates.RemoveRange(_context.CvCertificates.Where(cer => cer.Cvid == cv.Cvid));

            // Thêm mới
            model.Skills.ForEach(s => s.Cvid = cv.Cvid);
            model.Experiences.ForEach(e => e.Cvid = cv.Cvid);
            model.Educations.ForEach(ed => ed.Cvid = cv.Cvid);
            model.Certificates.ForEach(cer => cer.Cvid = cv.Cvid);

            _context.CvSkills.AddRange(model.Skills);
            _context.CvExperiences.AddRange(model.Experiences);
            _context.CvEducations.AddRange(model.Educations);
            _context.CvCertificates.AddRange(model.Certificates);

            _context.SaveChanges();

            ViewBag.SuccessMessage = "Cập nhật CV thành công!";
            return RedirectToAction("CV");
        }

        [HttpPost]
        public IActionResult DeleteCV()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return RedirectToAction("LoginCandidate", "Account");

            var cv = _context.Cvs.FirstOrDefault(c => c.CandidateId == candidateId.Value);
            if (cv == null) return NotFound();

            // Xóa các bảng con
            _context.CvSkills.RemoveRange(_context.CvSkills.Where(s => s.Cvid == cv.Cvid));
            _context.CvExperiences.RemoveRange(_context.CvExperiences.Where(e => e.Cvid == cv.Cvid));
            _context.CvEducations.RemoveRange(_context.CvEducations.Where(ed => ed.Cvid == cv.Cvid));
            _context.CvCertificates.RemoveRange(_context.CvCertificates.Where(cer => cer.Cvid == cv.Cvid));

            _context.Cvs.Remove(cv);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa CV!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult ToggleSaveJob(int jobId)
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
                return Json(new { success = false, message = "Bạn chưa đăng nhập" });

            // Debug xem jobId có tồn tại không
            var job = _context.Jobs.FirstOrDefault(j => j.JobId == jobId);
            if (job == null)
            {
                return Json(new { success = false, message = $"JobId {jobId} không tồn tại trong DB" });
            }

            var existing = _context.SavedJobs
                .FirstOrDefault(s => s.CandidateId == candidateId.Value && s.JobId == jobId);

            if (existing != null)
            {
                _context.SavedJobs.Remove(existing);
                _context.SaveChanges();
                return Json(new { success = true, action = "removed" });
            }

            var savedJob = new SavedJob
            {
                CandidateId = candidateId.Value,
                JobId = jobId,
                SavedAt = DateTime.Now
            };
            _context.SavedJobs.Add(savedJob);
            _context.SaveChanges();

            return Json(new { success = true, action = "added" });
        }


        public IActionResult SavedJobs()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            if (!candidateId.HasValue)
            {
                return RedirectToAction("ChooseLogin", "Account");
            }

            var savedJobs = _context.SavedJobs
                .Include(s => s.Job)
                    .ThenInclude(j => j.Company)
                .Include(s => s.Job.Province) // 👈 load luôn Province để khỏi null
                .Where(s => s.CandidateId == candidateId.Value)
                .Select(s => new SavedJobItem
                {
                    JobId = s.JobId,
                    JobTitle = s.Job.Title,
                    CompanyName = s.Job.Company.CompanyName,
                    CompanyLogo = s.Job.Company.Logo, // ✅ thêm dòng này
                    Location = s.Job.Location,
                    ProvinceName = s.Job.Province != null ? s.Job.Province.ProvinceName : "Chưa cập nhật",
                    //Salary = s.Job.Salary
                })
                .ToList();

            return View(savedJobs);
        }

        public async Task<IActionResult> AppliedJobs()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!candidateId.HasValue || role != "Candidate")
            {
                TempData["Error"] = "Bạn cần đăng nhập bằng tài khoản ứng viên!";
                return RedirectToAction("ChooseLogin", "Account");
            }

            var appliedJobs = await _context.Applications
                .Where(a => a.CandidateId == candidateId.Value)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Company)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Province)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Category)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new AppliedJobViewModel
                {
                    JobId = a.JobId,
                    Title = a.Job.Title,
                    CompanyName = a.Job.Company != null ? a.Job.Company.CompanyName : "Chưa cập nhật",
                    ProvinceName = a.Job.Province != null ? a.Job.Province.ProvinceName : "Chưa cập nhật",
                    CategoryName = a.Job.Category != null ? a.Job.Category.CategoryName : "Chưa cập nhật",
                    Salary = a.Job.Salary,
                    CreatedAt = a.Job.CreatedAt ?? DateTime.Now

                })
                .ToListAsync();

            return View(appliedJobs);
        }


        public async Task<IActionResult> SuggestedJobs()
        {
            var candidateId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (!candidateId.HasValue || role != "Candidate")
            {
                TempData["Error"] = "Bạn cần đăng nhập bằng tài khoản ứng viên!";
                return RedirectToAction("ChooseLogin", "Account");
            }

            // Lấy CV ứng viên
            var cv = await _context.Cvs
                .Include(c => c.CvSkills)
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId.Value);

            if (cv == null)
            {
                ViewBag.Message = "Bạn chưa có CV, vui lòng cập nhật hồ sơ để nhận gợi ý việc làm phù hợp.";
                return View(new List<SuggestedJobViewModel>());
            }

            // Lấy danh sách Job đã apply và saved
            var appliedJobIds = await _context.Applications
                .Where(a => a.CandidateId == candidateId.Value)
                .Select(a => a.JobId)
                .ToListAsync();

            var savedJobIds = await _context.SavedJobs
                .Where(s => s.CandidateId == candidateId.Value)
                .Select(s => s.JobId)
                .ToListAsync();

            // Kỹ năng & mục tiêu
            var skills = cv.CvSkills?.Select(s => s.SkillName.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();

            var goals = !string.IsNullOrEmpty(cv.CareerGoal)
                ? cv.CareerGoal.Split(new[] { ',', '-', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>();

            if (!skills.Any() && !goals.Any())
            {
                ViewBag.Message = "Bạn chưa thêm kỹ năng hoặc mục tiêu nghề nghiệp, vui lòng cập nhật hồ sơ.";
                return View(new List<SuggestedJobViewModel>());
            }

            // Lấy danh sách Job chưa apply hoặc saved và đã được duyệt
            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .Include(j => j.Province)
                .Where(j => !appliedJobIds.Contains(j.JobId) &&
                            !savedJobIds.Contains(j.JobId) &&
                            (j.IsApproved ?? false))
                .Include(j => j.Applications)
                .ToListAsync();

            // Tính điểm tương thích
            var suggestedJobs = jobs
                .Select(j =>
                {
                    int skillScore = skills.Count(s =>
                        (!string.IsNullOrEmpty(j.Title) && j.Title.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(j.Description) && j.Description.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(j.Requirements) && j.Requirements.Contains(s, StringComparison.OrdinalIgnoreCase))
                    );

                    int goalScore = goals.Count(g =>
                        (!string.IsNullOrEmpty(j.Title) && j.Title.Contains(g, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(j.Description) && j.Description.Contains(g, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(j.Requirements) && j.Requirements.Contains(g, StringComparison.OrdinalIgnoreCase))
                    );

                    int matchScore = skillScore + goalScore;

                    return new { Job = j, MatchScore = matchScore };
                })
                .Where(x => x.MatchScore > 0)
                .OrderByDescending(x => x.MatchScore)
                .ThenByDescending(x => x.Job.CreatedAt)
                .Select(x => new SuggestedJobViewModel
                {
                    JobId = x.Job.JobId,
                    Title = x.Job.Title,
                    JobType = x.Job.JobType,
                    CompanyName = x.Job.Company?.CompanyName ?? "Chưa cập nhật",
                    CompanyLogo = x.Job.Company?.Logo ?? "",
                    GenderRequirement = x.Job.GenderRequirement ?? "Không yêu cầu",
                    ExpireDate = x.Job.ExpireDate,
                    Location = x.Job.Location,
                    ProvinceName = x.Job.Province?.ProvinceName ?? "Chưa cập nhật",
                 ///   VacancyCount = x.Job.VacancyCount,
                    ApplicationsCount = x.Job.Applications?.Count() ?? 0,
                    Salary = string.IsNullOrEmpty(x.Job.Salary) ? "Liên hệ" : x.Job.Salary,
                    MatchScore = x.MatchScore,
                    JobImageUrl = x.Job.ImageUrl ?? "~/images/default-job.png"
                })
                .ToList();

            if (!suggestedJobs.Any())
            {
                ViewBag.Message = "Không có công việc nào phù hợp với hồ sơ của bạn hiện tại.";
            }

            return View(suggestedJobs);
        }

    }
}
