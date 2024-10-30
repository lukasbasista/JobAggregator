namespace JobAggregator.Api.Models
{
    public class Portal
    {
        public int PortalID { get; set; }
        public string PortalName { get; set; }
        public string BaseUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string? PortalLogoUrl { get; set; }

        public virtual ICollection<JobPosting> JobPostings { get; set; }
    }
}
