using System.Text.Json.Serialization;

namespace JobAggregator.Api.Models
{
    public class GptApiResponse
    {
        [JsonPropertyName("choices")]
        public List<GptChoice> Choices { get; set; } = new List<GptChoice>();
    }

    public class GptChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public GptMessage Message { get; set; } = new GptMessage();

        [JsonPropertyName("logprobs")]
        public object Logprobs { get; set; } = new object();

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;
    }

    public class GptMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class JobPostingData
    {
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public decimal? SalaryFrom { get; set; }
        public decimal? SalaryTo { get; set; }
        public string? JobType { get; set; }
        public string ApplyUrl { get; set; } = string.Empty;
    }

    public class CompanyData
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? FoundedYear { get; set; }
        public string? NumberOfEmployees { get; set; }
        public string? Headquarters { get; set; }
        public string? Industry { get; set; }
    }
}