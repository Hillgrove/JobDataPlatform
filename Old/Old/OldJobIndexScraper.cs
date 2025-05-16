using HtmlAgilityPack;
using Scrapers.Parser;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace Scrapers.Old
{
    internal static class OldJobIndexScraper
    {
        private const string RssBaseUrl = "https://www.jobindex.dk/jobsoegning.rss?geoareaid=1221&subid=1";
        private const string PageQueryParam = "page=";
        private const int ResultsPerPage = 20;

        internal static async Task Run()
        {
            using HttpClient HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "HojlundScraper/1.0 - Efter aftale med Daniel Egeberg (kontakt: jesper@hillgrove.dk)");

            int page = 1;
            int total = 0;
            bool isDone = false;
            while (!isDone)
            {
                var feed = await GetRssFeedAsync(HttpClient, page++);
                
                foreach (var item in feed.Items)
                {
                    await ProcessJobItemAsync(HttpClient, item);
                    await Task.Delay(1000);
                    total++;
                }

                isDone = feed.Items.Count() < ResultsPerPage;
                await Task.Delay(1000); // delay to avoid overwhelming the server
            }

            Console.WriteLine($"Total items processed: {total}");
        }

        private static async Task ProcessJobItemAsync(HttpClient httpClient, SyndicationItem item)
        {
            var summaryDoc = new HtmlDocument();
            summaryDoc.LoadHtml(item.Summary.Text);

            var location = summaryDoc.DocumentNode
                .SelectSingleNode("//*[contains(@class, 'jix_robotjob--area')]")?.InnerText.Trim();

            var resolvedPostalCode = location != null ? Location.Extract(location) : null;

            var internalJobUrl = item.Links[0].Uri.ToString();
            var html = await httpClient.GetStringAsync(internalJobUrl);

            var jobDoc = new HtmlDocument();
            jobDoc.LoadHtml(html);

            var linkNode = jobDoc.DocumentNode
                .SelectSingleNode("//a[contains(@class, 'seejobdesktop') or contains(@class, 'seejobmobil')]")
                ?? jobDoc.DocumentNode
                    .SelectSingleNode("//a[normalize-space(text())='Se jobbet']");

            var externalJobUrl = linkNode?.GetAttributeValue("href", null!);

            var paragraphs = summaryDoc.DocumentNode.SelectNodes("//p");
            var jobSummaryText = paragraphs != null
                ? string.Join(" ", paragraphs.Select(p => p.InnerText.Trim()))
                : string.Empty;

            var programmingLanguages = ProgrammingLanguage.Extract(jobSummaryText);


            Console.WriteLine($"Location: {location}");
            Console.WriteLine($"Internal Job URL: {internalJobUrl}");
            Console.WriteLine($"External Job URL: {externalJobUrl}");
            Console.WriteLine("TechStack: " + string.Join(", ", programmingLanguages));

            Console.WriteLine($"\n{new string('-', 10)}\n");
        }

        private static async Task<SyndicationFeed> GetRssFeedAsync(HttpClient httpClient, int page)
        {
            var url = $"{RssBaseUrl}&{PageQueryParam}{page}";
            using var stream = await httpClient.GetStreamAsync(url);
            return SyndicationFeed.Load(XmlReader.Create(stream));
        }

        private static string CleanHtml(HtmlDocument doc)
        {
            // Fjern script og style nodes
            doc.DocumentNode.SelectNodes("//script|//style")?.ToList().ForEach(n => n.Remove());

            // Få kun den synlige tekst
            var rawText = doc.DocumentNode.InnerText;

            // Fjern overflødige whitespaces, linjeskift etc.
            return Regex.Replace(rawText, @"\s+", " ").Trim();
        }
    }
}

