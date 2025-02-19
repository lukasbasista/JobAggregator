using HtmlAgilityPack;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobAggregator.Api.Services.Implementations
{
    public class JobsContactScraper : BaseScraper
    {
        public override Portal PortalInfo { get; } = new Portal
        {
            PortalName = "JobsContact.cz",
            BaseUrl = "https://www.jobscontact.cz",
            PortalLogoUrl = "https://media.zivefirmy.cz/logo/621b548ce462ba3994ad66a6b8bf66.jpg",
        };

        public JobsContactScraper(IServiceProvider serviceProvider, ILogger<JobsContactScraper> logger, IGptJobParser gptJobParser)
            : base(serviceProvider, logger, gptJobParser)
        {
            _httpClient.BaseAddress = new Uri(PortalInfo.BaseUrl);
        }

        protected override string GetPageUrl(int pageNumber) => $"/prace?page={pageNumber}";

        protected override async Task<IEnumerable<HtmlNode>> GetJobListingsAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nepodarilo sa načítať stránku {Url}. StatusCode: {StatusCode}", url, response.StatusCode);
                return Enumerable.Empty<HtmlNode>();
            }

            var pageContents = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContents);

            var jobNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'job-box')]");
            return jobNodes ?? Enumerable.Empty<HtmlNode>();
        }

        protected override string ExtractJobUrl(HtmlNode jobListing)
        {
            var linkNode = jobListing.SelectSingleNode(".//a[@href]");
            var relativeUrl = linkNode?.GetAttributeValue("href", null);

            if (string.IsNullOrEmpty(relativeUrl))
                return null;

            return relativeUrl.StartsWith("http")
                ? relativeUrl
                : $"{PortalInfo.BaseUrl.TrimEnd('/')}{relativeUrl}";
        }

        protected override async Task<string> GetJobContentAsync(string jobUrl)
        {
            var response = await _httpClient.GetAsync(jobUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nepodarilo sa načítať detail inzerátu z URL: {JobUrl}. StatusCode: {StatusCode}", jobUrl, response.StatusCode);
                return null;
            }

            var pageContent = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);

            var contentNode = htmlDoc.DocumentNode.SelectSingleNode("//section[@class='new-detail']");
            if (contentNode != null)
            {
                var nodesToRemove = contentNode.SelectNodes(".//header|.//footer|.//form|.//div[contains(@class,'alike-job')]|.//div[contains(@class,'help')]|.//div[contains(@class,'next-info')]|.//script|.//style|.//div[contains(@class,'end-box')]");
                if (nodesToRemove != null)
                {
                    foreach (var node in nodesToRemove)
                        node.Remove();
                }

                var mainContent = contentNode.SelectNodes(".//div[@class='job-title']|.//div[@class='job-content']//h1|.//div[@class='job-content']//h2|.//div[@class='job-content']//ul|.//div[@class='job-content']//p|.//div[@class='job-content']//div[@class='nabidka']");
                if (mainContent != null && mainContent.Any())
                {
                    var cleanedContent = string.Join("\n", mainContent.Select(node =>
                        HtmlEntity.DeEntitize(node.InnerText)
                            .Replace("\r", "")
                            .Replace("\n", " ")
                            .Trim()));
                    return cleanedContent;
                }
            }
            return string.Empty;
        }
    }
}
