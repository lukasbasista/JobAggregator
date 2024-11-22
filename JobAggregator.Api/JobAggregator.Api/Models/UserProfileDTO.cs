namespace JobAggregator.Api.Models
{
    public class UserProfileDTO
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Location { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime AccountCreated { get; set; }
        public DateTime LastLogin { get; set; }
    }

}
