using System;

namespace DoanWeb.Models
{
    public class SuggestedJobViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = "";
        public string JobType { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string CompanyLogo { get; set; } = "";
        public string GenderRequirement { get; set; } = "";
        public DateTime? ExpireDate { get; set; }
        public string Location { get; set; } = "";
        public string ProvinceName { get; set; } = "";
        public int VacancyCount { get; set; }
        public int ApplicationsCount { get; set; } = 0; // số ứng viên đã apply
        public string Salary { get; set; } = "";
        public int MatchScore { get; set; } = 0; // độ phù hợp nếu dùng gợi ý
        public string JobImageUrl { get; set; } = ""; // ảnh công việc
    }
}
