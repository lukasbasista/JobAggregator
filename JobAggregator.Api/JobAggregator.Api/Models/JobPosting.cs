using System.ComponentModel.DataAnnotations.Schema;

namespace JobAggregator.Api.Models
{
    public class JobPosting
    {
        public int JobPostingID { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryFrom { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryTo { get; set; }
        public string Currency { get; set; }
        public string JobType { get; set; }
        public string ApplyUrl { get; set; }
        public string ExternalID { get; set; }
        public DateTime DateScraped { get; set; }
        public string HashCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public int? CompanyID { get; set; }
        public virtual Company Company { get; set; }

        public int PortalID { get; set; }
        public virtual Portal Portal { get; set; }
    }
}
