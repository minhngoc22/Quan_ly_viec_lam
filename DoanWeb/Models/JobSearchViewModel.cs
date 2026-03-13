using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoanWeb.Models
{
    public class JobSearchViewModel
    {
        
            public string Keyword { get; set; }
            public int? ProvinceId { get; set; }

            // Dùng để hiển thị danh sách các tỉnh
            public List<SelectListItem> Provinces { get; set; }

            // Kết quả tìm kiếm
            public List<Job> Jobs { get; set; }

        public List<SelectListItem> Categories { get; set; }   // Thêm

        public int? CategoryId { get; set; }                  // Thêm

        // Thêm danh sách công ty
        public IEnumerable<Company>? TopCompanies { get; set; }

        public bool IsFavorite { get; set; } = false;
    }
    }
