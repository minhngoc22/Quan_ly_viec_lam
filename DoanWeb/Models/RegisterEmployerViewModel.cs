using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DoanWeb.Models
{
    public class RegisterEmployerViewModel
    {
        // Thông tin người đại diện
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Nhập lại mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; }

        // Thông tin công ty
        [Required(ErrorMessage = "Tên công ty là bắt buộc")]
        public string CompanyName { get; set; }

        // ID lĩnh vực (Job Category)
        [Required(ErrorMessage = "Lĩnh vực là bắt buộc")]
        public int CategoryID { get; set; }

        // ID tỉnh/thành phố
        [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
        public int ProvinceId { get; set; }



        [EmailAddress(ErrorMessage = "Email công ty không hợp lệ")]
        public string CompanyEmail { get; set; }

        public string Address { get; set; }

        // Danh sách chọn sẵn
        public IEnumerable<SelectListItem> JobCategories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Provinces { get; set; } = new List<SelectListItem>();

        public string Website { get; set; }
        public string Description { get; set; }
    }
}
