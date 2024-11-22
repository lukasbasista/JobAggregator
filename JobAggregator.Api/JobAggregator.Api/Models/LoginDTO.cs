using System.ComponentModel.DataAnnotations;

namespace JobAggregator.Api.Models
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }

}
