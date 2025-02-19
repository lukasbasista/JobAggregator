namespace JobAggregator.Api.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string? FoundedYear { get; set; }
        public string? Headquarters { get; set; }
        public string? Industry { get; set; }
        public string? NumberOfEmployees { get; set; }

        public virtual ICollection<JobPosting>? JobPostings { get; set; }
    }
}
