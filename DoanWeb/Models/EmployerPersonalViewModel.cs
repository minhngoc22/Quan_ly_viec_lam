using System.ComponentModel.DataAnnotations;

namespace DoanWeb.Models
{
    public class EmployerPersonalViewModel
    {
        public int EmployerId { get; set; }

        // Họ và tên
        [Required(ErrorMessage = "Tên nhân viên không được để trống")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string FullName { get; set; }

        // Số điện thoại
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        public string Phone { get; set; }

        // Chức vụ
        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [StringLength(50, ErrorMessage = "Chức vụ tối đa 50 ký tự")]
        public string Position { get; set; }

        // Email
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; }

        // Mật khẩu cũ (để xác nhận)
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; }

        // Mật khẩu mới (tuỳ chọn)
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
        public string NewPassword { get; set; }
    }
}
