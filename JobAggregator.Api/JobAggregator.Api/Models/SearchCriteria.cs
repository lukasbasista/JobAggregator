namespace JobAggregator.Api.Models
{
    public class SearchCriteria
    {
        public string? Keywords { get; set; }
        public string? Location { get; set; }
        public string? CompanyName { get; set; }
        public string? JobType { get; set; }
        public DateTime? CreatedDate { get; set; }
        // public string SalaryRange { get; set; }
        // public DateTime? PostedAfter { get; set; }
        // public DateTime? PostedBefore { get; set; }
    }
}
