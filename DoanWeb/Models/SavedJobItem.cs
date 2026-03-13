namespace DoanWeb.Models
{
    public class SavedJobItem
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string? Location { get; set; }
        public string? ProvinceName { get; set; }
        public string? Salary { get; set; }


        // ✅ Thêm logo
        public string? CompanyLogo { get; set; }
    }

}
