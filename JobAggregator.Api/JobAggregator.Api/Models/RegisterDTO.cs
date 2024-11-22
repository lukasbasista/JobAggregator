using System.ComponentModel.DataAnnotations;

namespace JobAggregator.Api.Models
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string? LastName { get; set; }
        public string? Location { get; set; }
    }
}
