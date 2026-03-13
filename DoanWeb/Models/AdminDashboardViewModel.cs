namespace DoanWeb.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalJobsPending { get; set; }
        public int TotalReports { get; set; }
        public int TotalEmployers { get; set; } // 👈 thêm cái này

        // ✅ Thêm dòng này
        public int TotalJobComments { get; set; }

        // ✅ Thêm TotalCareerGuides
        public int TotalCareerGuides { get; set; }

        // 🔥 Thêm
        public int TotalReportsPending { get; set; }
        public List<User> RecentUsers { get; set; }
        public List<Job> PendingJobs { get; set; }
        public List<Report> RecentReports { get; set; }


        // 🔥 Top 5 Job có nhiều bình luận nhất
        public List<JobCommentStats> TopCommentedJobs { get; set; }

        public class JobCommentStats
        {
            public int JobId { get; set; }
            public string Title { get; set; }
            public int CommentCount { get; set; }
        }

    }
}
