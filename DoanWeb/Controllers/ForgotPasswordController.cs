using DoanWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DoanWeb.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly JobContext _context;
        private readonly EmailService _emailService;

        public ForgotPasswordController(JobContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ============================
        // B1: Nhập email để gửi OTP
        // ============================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống.";
                return View();
            }

            // Sinh OTP ngẫu nhiên 6 số
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào DB
            var token = new PasswordResetToken
            {
                UserId = user.UserId,
                Otp = otp,
                ExpiryTime = DateTime.Now.AddMinutes(10),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(token);
            _context.SaveChanges();

            // Gửi OTP qua email
            _emailService.SendEmail(user.Email, "Mã OTP đặt lại mật khẩu", $"Mã OTP của bạn là: {otp}");

            TempData["Success"] = "Đã gửi OTP về email. Vui lòng kiểm tra hộp thư.";
            TempData["Email"] = user.Email;

            return RedirectToAction("VerifyOtpAndReset");
        }

        // ============================
        // B2: Nhập OTP + mật khẩu mới
        // ============================
        [HttpGet]
        public IActionResult VerifyOtpAndReset()
        {
            ViewBag.Email = TempData["Email"];
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtpAndReset(string email, string otp, string newPassword, string confirmPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Email không hợp lệ.";
                ViewBag.Email = email;
                return View();
            }
            // Cách 1: So sánh với false
            var token = _context.PasswordResetTokens
                .FirstOrDefault(t => t.UserId == user.UserId
                                  && t.Otp == otp
                                  && t.IsUsed == false  // <-- thay !t.IsUsed
                                  && t.ExpiryTime > DateTime.Now);

            if (token == null)
            {
                TempData["Error"] = "OTP không đúng hoặc đã hết hạn.";
                ViewBag.Email = email;
                return View();
            }


            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp.";
                ViewBag.Email = email;
                return View();
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = HashPassword(newPassword);

            // Đánh dấu OTP đã dùng
            token.IsUsed = true;
            _context.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập.";
            return RedirectToAction("ChooseLogin", "Account");
        }

        // ============================
        // Hàm hash mật khẩu
        // ============================
        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
