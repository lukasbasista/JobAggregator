namespace JobAggregator.Api.Models
{
    public class JobPosting
    {
        public int JobPostingID { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime PostedDate { get; set; }
        public string ApplyUrl { get; set; }
        public int PortalID { get; set; }
        public string ExternalID { get; set; }
        public DateTime DateScraped { get; set; }
        public string HashCode { get; set; }
        public string Salary { get; set; }
        public string JobType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public virtual Portal Portal { get; set; }
    }
}
