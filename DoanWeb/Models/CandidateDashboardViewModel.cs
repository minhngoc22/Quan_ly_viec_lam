using System;
using System.Collections.Generic;

namespace DoanWeb.Models
{
    public class CandidateDashboardViewModel
    {
        // Thông tin cá nhân
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
        // Applications
        public List<ApplicationItem> Applications { get; set; } = new();

        // Saved Jobs
        public List<SavedJobItem> SavedJobs { get; set; } = new();

        // Cover Letters
        public List<CoverLetterItem> CoverLetters { get; set; } = new();
    }

    public class ApplicationItem
    {
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Status { get; set; }
    }



    public class CoverLetterItem
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
