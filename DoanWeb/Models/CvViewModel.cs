namespace DoanWeb.Models
{
    public class CvViewModel
    {
        public Cv Cv { get; set; } = new Cv();  // Hồ sơ chính
        public List<CvSkill> Skills { get; set; } = new();  // Danh sách kỹ năng
        public List<CvExperience> Experiences { get; set; } = new();  // Kinh nghiệm
        public List<CvEducation> Educations { get; set; } = new();  // Học vấn
        public List<CvCertificate> Certificates { get; set; } = new();  // Chứng chỉ

        // 🔹 Thêm avatar
        public string Avatar { get; set; }
        public string AvatarColor { get; set; }

    }
}
