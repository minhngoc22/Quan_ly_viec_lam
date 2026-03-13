using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DoanWeb.Models
{
    public class JobFormViewModel
    {
        public Job Job { get; set; } = new Job();

        public List<Job> ImportedJobs { get; set; } = new List<Job>();

        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Companies { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Provinces { get; set; } = new List<SelectListItem>(); // khởi tạo sẵn để tránh null

        // ✅ Thêm property để upload ảnh
        public IFormFile? ImageFile { get; set; }
    }
}
