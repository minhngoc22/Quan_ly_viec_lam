namespace DoanWeb.Models
{
    public class ApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }

        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string CVPath { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; } // Pending | Approved | Rejected
    }
}
