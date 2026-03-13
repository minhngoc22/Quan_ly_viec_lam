namespace DoanWeb.Models
{
    public class AppliedJobViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = "";
        public string CompanyName { get; set; } = "Chưa cập nhật";
        public string ProvinceName { get; set; } = "Chưa cập nhật";
        public string CategoryName { get; set; } = "Chưa cập nhật";
        public string Salary { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
