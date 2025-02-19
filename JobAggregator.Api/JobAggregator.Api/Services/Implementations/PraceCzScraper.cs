using HtmlAgilityPack;
using JobAggregator.Api.Models;
using JobAggregator.Api.Services.Interfaces;
using Ganss.Xss;
using Microsoft.Playwright;
using PlaywrightCookie = Microsoft.Playwright.Cookie;
using System.Text;

namespace JobAggregator.Api.Services.Implementations
{
    public class PraceCzScraper : BaseScraper
    {
        public override Portal PortalInfo { get; } = new Portal
        {
            PortalName = "Prace.cz",
            BaseUrl = "https://www.prace.cz",
            PortalLogoUrl = "https://pracecdn.cz/images/logo-beata.svg",
        };

        public PraceCzScraper(IServiceProvider serviceProvider, ILogger<PraceCzScraper> logger, IGptJobParser gptJobParser)
            : base(serviceProvider, logger, gptJobParser)
        {
            _httpClient.BaseAddress = new Uri(PortalInfo.BaseUrl);
        }

        protected override string GetPageUrl(int pageNumber) => $"/nabidky/?page={pageNumber}";

        protected override int MaxPages => 1;

        protected override async Task<IEnumerable<HtmlNode>> GetJobListingsAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to load page {Url}. StatusCode: {StatusCode}", url, response.StatusCode);
                return Enumerable.Empty<HtmlNode>();
            }

            var pageContents = await response.Content.ReadAsStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContents);

            var jobNodes = htmlDoc.DocumentNode.SelectNodes("//li[contains(@class, 'search-result__advert')]");
            return jobNodes != null ? jobNodes.Cast<HtmlNode>() : Enumerable.Empty<HtmlNode>();
        }

        protected override string? ExtractJobUrl(HtmlNode jobListing)
        {
            var titleNode = jobListing.SelectSingleNode(".//a[@data-qa='search-result-position-title']");
            if (titleNode == null)
            {
                titleNode = jobListing.SelectSingleNode(".//h3[contains(@class, 'half-standalone')]/a");
            }
            if (titleNode == null)
            {
                titleNode = jobListing.SelectSingleNode(".//a[@href]");
            }
            var relativeUrl = titleNode?.GetAttributeValue("href", null);
            if (string.IsNullOrEmpty(relativeUrl))
            {
                _logger.LogWarning("Could not retrieve URL from job listing.");
                return null;
            }
            var absoluteUrl = relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? relativeUrl
                : $"{PortalInfo.BaseUrl.TrimEnd('/')}{relativeUrl}";
            try
            {
                var uriBuilder = new UriBuilder(absoluteUrl)
                {
                    Query = string.Empty,
                    Fragment = string.Empty
                };
                return uriBuilder.Uri.ToString();
            }
            catch (UriFormatException)
            {
                _logger.LogWarning("Invalid URL format: {Url}", absoluteUrl);
                return absoluteUrl;
            }
        }

        protected override async Task<string> GetJobContentAsync(string jobUrl)
        {
            var response = await _httpClient.GetAsync(jobUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to load {Url} (StatusCode {Code}), switching to Playwright", jobUrl, response.StatusCode);
                return await GetExternalJobContentWithPlaywrightAsync(jobUrl);
            }

            var pageContent = await response.Content.ReadAsStringAsync();
            var finalUri = response.RequestMessage?.RequestUri ?? new Uri(jobUrl);

            bool isExternalDomain = !finalUri.Host.EndsWith("prace.cz", StringComparison.OrdinalIgnoreCase);

            if (isExternalDomain)
            {
                _logger.LogInformation("External website (domain={Domain}), downloading via Playwright", finalUri.Host);
                return await GetExternalJobContentWithPlaywrightAsync(finalUri.ToString());
            }

            if (string.IsNullOrWhiteSpace(pageContent) || pageContent.Length < 200)
            {
                _logger.LogWarning("Content from prace.cz is too short ({Length} characters). Attempting fallback with Playwright on {Url}", pageContent?.Length, finalUri);
                return await GetExternalJobContentWithPlaywrightAsync(finalUri.ToString());
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);

            var extracted = ProcessInternalDomainContent(htmlDoc);
            if (string.IsNullOrWhiteSpace(extracted))
            {
                _logger.LogWarning("Extracted content is empty (prace.cz). Fallback to Playwright. URL={Url}", finalUri);
                return await GetExternalJobContentWithPlaywrightAsync(finalUri.ToString());
            }

            return extracted;
        }

        private async Task<string> GetExternalJobContentWithPlaywrightAsync(string externalUrl)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(externalUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });
            }
            catch (TimeoutException tex)
            {
                _logger.LogWarning("Goto timed out after 60s for URL={Url}. {Message}", externalUrl, tex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Goto threw an exception for URL={Url}. {Message}", externalUrl, ex.Message);
            }

            try
            {
                await page.WaitForSelectorAsync(".cp-loader", new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Detached,
                    Timeout = 10000
                });
            }
            catch
            {
                _logger.LogWarning("Could not remove loader '.cp-loader' from DOM within 10s, proceeding anyway.");
            }

            try
            {
                await page.WaitForSelectorAsync("div.cp-detail__content, div.col-xs-12.headline, .hero__title",
                    new PageWaitForSelectorOptions
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = 25000
                    });
            }
            catch
            {
                _logger.LogWarning("None of the main content selectors were found within 25s, continuing with partial content.");
            }

            await page.WaitForTimeoutAsync(1000);

            var content = await page.ContentAsync();
            await browser.CloseAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            var extracted = ProcessExternalDomainContent(doc);

            return extracted;
        }

        private async Task<string> GetJobContentWithPlaywrightAsync(string jobUrl)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(jobUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            string finalUrl = page.Url;
            var finalHost = new Uri(finalUrl).Host;
            string cookieDomain = finalHost.StartsWith(".") ? finalHost : "." + finalHost;
            long cookieExpiration = DateTimeOffset.Parse("2026-02-15T18:04:27Z").ToUnixTimeSeconds();

            string levelPart = finalHost.Equals("o2.jobs.cz", StringComparison.OrdinalIgnoreCase)
                ? "%2C%22analytics%22%2C%22functionality%22%2C%22ad%22%2C%22personalization%22"
                : string.Empty;
            string cookieValue = "%7B%22level%22%3A%5B%22necessary%22" + levelPart +
                                 "%5D%2C%22revision%22%3A1%2C%22data%22%3A%7B%22serviceName%22%3A%22CP_" + finalHost +
                                 "%22%2C%22uid%22%3A%22ZZwnHpVc0CsRZS6AkDcwA%22%7D%2C%22rfc_cookie%22%3Atrue%7D";

            var cookie = new PlaywrightCookie
            {
                Name = "lmc_ccm",
                Value = cookieValue,
                Domain = cookieDomain,
                Path = "/",
                Expires = cookieExpiration,
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteAttribute.Lax
            };

            await context.AddCookiesAsync(new[] { cookie });

            await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

            try
            {
                var contentElement = await page.WaitForSelectorAsync("section.cp-detail", new PageWaitForSelectorOptions { Timeout = 15000 });
                if (contentElement != null)
                {
                    return await contentElement.InnerHTMLAsync();
                }
            }
            catch
            {
            }

            await page.WaitForTimeoutAsync(5000);
            var content = await page.ContentAsync();
            await browser.CloseAsync();
            return content;
        }

        private string ProcessExternalDomainContent(HtmlDocument htmlDoc)
        {
            _logger.LogDebug("External web");
            var sections = htmlDoc.DocumentNode.SelectNodes("//main");
            if (sections == null || sections.Count == 0)
            {
                sections = htmlDoc.DocumentNode.SelectNodes("//section[contains(@class, 'cp-detail')]");
            }
                HtmlNode contentNode;
            if (sections == null || sections.Count == 0)
            {
                contentNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var sec in sections)
                {
                    sb.Append(sec.InnerHtml);
                    sb.Append("\n");
                }

                var tempDoc = new HtmlDocument();
                tempDoc.LoadHtml(sb.ToString());
                contentNode = tempDoc.DocumentNode;
            }
            if (contentNode == null)
            {
                contentNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
            }
            if (contentNode == null)
                return string.Empty;

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("div");
            sanitizer.AllowedTags.Add("span");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("h1");
            sanitizer.AllowedTags.Add("h2");
            sanitizer.AllowedTags.Add("h3");
            sanitizer.AllowedTags.Add("h4");
            sanitizer.AllowedTags.Add("h5");
            sanitizer.AllowedTags.Add("h6");
            sanitizer.AllowedTags.Add("article");
            sanitizer.AllowedTags.Add("section");
            sanitizer.AllowedTags.Add("blockquote");
            sanitizer.AllowedTags.Add("pre");
            sanitizer.AllowedTags.Add("code");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("ol");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("br");
            sanitizer.AllowedTags.Add("strong");
            sanitizer.AllowedTags.Add("em");
            sanitizer.AllowedTags.Add("a");
            sanitizer.AllowedTags.Add("table");
            sanitizer.AllowedTags.Add("thead");
            sanitizer.AllowedTags.Add("tbody");
            sanitizer.AllowedTags.Add("tfoot");
            sanitizer.AllowedTags.Add("tr");
            sanitizer.AllowedTags.Add("th");
            sanitizer.AllowedTags.Add("td");

            var nodesToRemove = htmlDoc.DocumentNode.SelectNodes(
                "//*[contains(local-name(), 'script') or contains(local-name(), 'style') or contains(local-name(), 'img') or contains(local-name(), 'svg') or " +
                "contains(@class, 'footer') or contains(@class, 'header') or starts-with(@id, 'cc--')]");

            nodesToRemove?.ToList().ForEach(n => n.Remove());
            var sanitized = sanitizer.Sanitize(contentNode.InnerHtml);

            return HtmlEntity.DeEntitize(sanitized).Trim();
        }

        private string ProcessInternalDomainContent(HtmlDocument htmlDoc)
        {
            var nodesToRemove = htmlDoc.DocumentNode.SelectNodes(
                "//header | //footer | //nav | //script | //style | " +
                "//div[contains(@class, 'header')] | //div[contains(@class, 'footer')] | " +
                "//div[contains(@class, 'navigation')] | //div[contains(@class, 'apply-form')] | " +
                "//div[contains(@class, 'social-media')] | //div[contains(@class, 'recommendation')]");
            nodesToRemove?.ToList().ForEach(n => n.Remove());

            var contentNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'grid')]");
            if (contentNodes == null || !contentNodes.Any(n => !string.IsNullOrWhiteSpace(n.InnerHtml)))
            {
                _logger.LogWarning("Primary content extraction failed, trying alternative selector 'div[contains(@class, 'job-content')]'.");
                contentNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'job-content')]");
            }
            if (contentNodes == null || !contentNodes.Any(n => !string.IsNullOrWhiteSpace(n.InnerHtml)))
            {
                _logger.LogWarning("Alternative extraction failed, using entire <body> content as fallback.");
                contentNodes = htmlDoc.DocumentNode.SelectNodes("//body");
            }

            var sb = new StringBuilder();
            foreach (var node in contentNodes)
            {
                sb.Append(node.InnerHtml);
                sb.Append("\n");
            }
            var combinedHtml = sb.ToString();

            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("div");
            sanitizer.AllowedTags.Add("span");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("h1");
            sanitizer.AllowedTags.Add("h2");
            sanitizer.AllowedTags.Add("h3");
            sanitizer.AllowedTags.Add("h4");
            sanitizer.AllowedTags.Add("h5");
            sanitizer.AllowedTags.Add("h6");
            sanitizer.AllowedTags.Add("article");
            sanitizer.AllowedTags.Add("section");
            sanitizer.AllowedTags.Add("blockquote");
            sanitizer.AllowedTags.Add("pre");
            sanitizer.AllowedTags.Add("code");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("ol");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("br");
            sanitizer.AllowedTags.Add("strong");
            sanitizer.AllowedTags.Add("em");
            sanitizer.AllowedTags.Add("a");
            sanitizer.AllowedTags.Add("table");
            sanitizer.AllowedTags.Add("thead");
            sanitizer.AllowedTags.Add("tbody");
            sanitizer.AllowedTags.Add("tfoot");
            sanitizer.AllowedTags.Add("tr");
            sanitizer.AllowedTags.Add("th");
            sanitizer.AllowedTags.Add("td");

            var sanitized = sanitizer.Sanitize(combinedHtml);
            var finalContent = HtmlEntity.DeEntitize(sanitized).Trim();
            if (finalContent.Length < 100)
            {
                _logger.LogWarning("Extracted content is very short: {Length} characters.", finalContent.Length);
            }
            return finalContent;
        }
    }
}
