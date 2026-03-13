using System.ComponentModel.DataAnnotations;

namespace DoanWeb.Models
{
    public class CompanyViewModel
    {
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Tên công ty không được để trống")]
        [StringLength(200, ErrorMessage = "Tên công ty tối đa 200 ký tự")]
        public string CompanyName { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        public string Description { get; set; }

        [StringLength(300, ErrorMessage = "Địa chỉ tối đa 300 ký tự")]
        public string Address { get; set; }

        public string Website { get; set; }

        // 🏢 Logo (đường dẫn trong DB)
        public string? Logo { get; set; }


        // Upload file (form input)
        public IFormFile? LogoFile { get; set; }
        public IFormFile? LicenseFile { get; set; }
    }
}
