using Microsoft.AspNetCore.Identity;

namespace JobAggregator.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Location { get; set; }
        public DateTime AccountCreated { get; set; }
        public DateTime LastLogin { get; set; }
    }

}
