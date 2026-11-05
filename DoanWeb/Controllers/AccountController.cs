using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace DoanWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly JobContext _context;

        public AccountController(JobContext context)
        {
            _context = context;
        }

        // 👉 Trang đăng nhập mặc định (chuyển hướng sang chọn vai trò)
        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("ChooseLogin");
        }

        // 👉 Trang đăng ký mặc định (chuyển hướng sang chọn vai trò)
        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("ChooseRegister");
        }

        // 👉 Trang trung gian để chọn loại đăng nhập
        [HttpGet]
        public IActionResult ChooseLogin()
        {
            return View();
        }

        // 👉 Trang trung gian để chọn loại đăng nhập
        [HttpGet]
        public IActionResult ChooseRegister()
        {
            return View();
        }
        // ========== NHÀ TUYỂN DỤNG ==========
        [HttpGet]
        public IActionResult LoginEmployer()
        {
            ViewBag.Role = "Employer";
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult LoginEmployer(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedInputPassword = ComputeSha256Hash(model.Password);

                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email
                                         && (u.PasswordHash == model.Password || u.PasswordHash == hashedInputPassword) // so sánh cả 2
                                         && u.Role == "Employer");

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);

                    return RedirectToAction("Dashboard", "Employer");
                }

                ModelState.AddModelError("", "Sai email hoặc mật khẩu!");
            }

            ViewBag.Role = "Employer";
            return View(model);
        }


        // ========== ỨNG VIÊN ==========
        [HttpGet]
        public IActionResult LoginCandidate()
        {
            ViewBag.Role = "Candidate";
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult LoginCandidate(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedInputPassword = ComputeSha256Hash(model.Password);

                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email
                                         && (u.PasswordHash == model.Password || u.PasswordHash == hashedInputPassword) // so sánh cả 2
                                         && u.Role == "Candidate");

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId); // ✅ Lưu int
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Sai email hoặc mật khẩu!");
            }

            ViewBag.Role = "Candidate";
            return View(model);
        }

        //nhà tuyển dụng đăng ký
        // GET: Register Employer
        [HttpGet]
        public async Task<IActionResult> RegisterEmployer()
        {
            var model = new RegisterEmployerViewModel
            {
                JobCategories = await _context.JobCategories
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName })
                    .ToListAsync(),
                Provinces = await _context.Provinces
                    .Select(p => new SelectListItem { Value = p.ProvinceId.ToString(), Text = p.ProvinceName })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEmployer(RegisterEmployerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Load dropdown lại
                model.JobCategories = await _context.JobCategories
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName })
                    .ToListAsync();
                model.Provinces = await _context.Provinces
                    .Select(p => new SelectListItem { Value = p.ProvinceId.ToString(), Text = p.ProvinceName })
                    .ToListAsync();
                return View(model);
            }

            // Kiểm tra email tồn tại
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký");
                return View(model);
            }

            // Hash mật khẩu
            string hashedPassword = ComputeSha256Hash(model.Password);

            // 1️⃣ Tạo User
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Role = "Employer",
                CreatedAt = DateTime.Now,
                Status = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // cần để UserId

            // 2️⃣ Tạo Employer
            var employer = new Employer
            {
                CompanyName = model.CompanyName,
                Position = model.Position,
                Phone = model.Phone,
                EmployerNavigation = user
            };
            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();

            // 3️⃣ Tạo Company
            var company = new Company
            {
                CompanyName = model.CompanyName,
                EmployerId = employer.EmployerId,
                CategoryId = int.Parse(model.JobCategories.FirstOrDefault(c => c.Selected)?.Value ?? "1"),
                ProvinceId = int.Parse(model.Provinces.FirstOrDefault(p => p.Selected)?.Value ?? "1"),
                Address = model.Address,
                Website = model.Website,
                Description = model.Description,
                CreatedAt = DateTime.Now
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // 🔹 Thông báo thành công
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";

            // Trả về lại view để hiển thị thông báo
            return View(model);
        }



        // ================= Candidate =================
        [HttpGet] public IActionResult RegisterCandidate() => View(new CandidateRegisterViewModel());

        [HttpPost]
        public IActionResult RegisterCandidate(CandidateRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = ComputeSha256Hash(model.Password),
                Role = "Candidate",
                Status = true
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var candidate = new Candidate
            {
                CandidateId = user.UserId,
                Phone = model.Phone,
                Address = model.Address,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth.HasValue
                    ? DateOnly.FromDateTime(model.DateOfBirth.Value)
                    : null
            };
            _context.Candidates.Add(candidate);
            _context.SaveChanges();

            // Thông báo thành công ngay trên trang đăng ký
            ViewBag.SuccessMessage = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";

            // Xóa form (tùy chọn)
            ModelState.Clear();
            return View(new CandidateRegisterViewModel());
        }


        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }


        [HttpGet]
        public IActionResult LoginAdmin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedInputPassword = ComputeSha256Hash(model.Password);

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email
                                              && (u.PasswordHash == model.Password || u.PasswordHash == hashedInputPassword)
                                              && u.Role == "Admin");

                if (user != null && user.Status == true)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName); // ✅ Đổi FullName -> UserName

                    return RedirectToAction("Dashboard", "Admin");
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, hoặc tài khoản bị khóa.");
            }

            return View(model);
        }

        // ========== ĐĂNG XUẤT ==========
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
