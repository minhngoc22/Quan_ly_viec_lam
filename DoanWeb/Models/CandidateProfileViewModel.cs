namespace DoanWeb.Models
{
    public class CandidateProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }

        // CV
        public string Skills { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public string CareerGoal { get; set; }
        public string CVFile { get; set; }

        // 🔹 Thêm avatar
        public string Avatar { get; set; }
        public string AvatarColor { get; set; }

        // 🔹 File upload
        public IFormFile AvatarFile { get; set; }
    }

}
