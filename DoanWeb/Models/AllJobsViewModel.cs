namespace DoanWeb.Models
{
    public class AllJobsViewModel
    {
        public string Keyword { get; set; }
        public List<int> SelectedProvinceIds { get; set; } = new List<int>();
        public string SelectedJobType { get; set; }

        // 🔹 Khoảng lương đã chọn
        public int? SelectedSalaryRangeId { get; set; }

        // 🔹 Danh sách job hiển thị
        public List<Job> Jobs { get; set; } = new List<Job>();

        // 🔹 Dropdown filter
        public List<Province> Provinces { get; set; } = new List<Province>();
        public List<string> JobTypes { get; set; } = new List<string>();

        // 🔹 Danh sách khoảng lương
        public List<SalaryRange> SalaryRanges { get; set; } = SalaryRange.GetSalaryRanges();

        // 🔹 Danh mục công việc (mới thêm)
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public List<JobCategory> Categories { get; set; } = new List<JobCategory>();
    }

    public class SalaryRange
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }

        public static List<SalaryRange> GetSalaryRanges()
        {
            return new List<SalaryRange>
            {
                new SalaryRange { Id = 1, Label = "< 5 triệu", Min = null, Max = 5000000 },
                new SalaryRange { Id = 2, Label = "5 - 10 triệu", Min = 5000000, Max = 10000000 },
                new SalaryRange { Id = 3, Label = "10 - 20 triệu", Min = 10000000, Max = 20000000 },
                new SalaryRange { Id = 4, Label = "> 20 triệu", Min = 20000000, Max = null }
            };
        }
    }
}
