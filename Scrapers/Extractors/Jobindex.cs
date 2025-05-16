using HtmlAgilityPack;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Xml;

namespace Scrapers.Extractors
{
    internal static class Jobindex
    {
        private const string RssUrl             = "https://www.jobindex.dk/jobsoegning.rss?geoareaid=1221&subid=1";
        private const string PageQueryParam     = "page=";

        private const string OutputDir          = "data/raw";
        private const string PageDir            = $"{OutputDir}/jobindexPages";

        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };


        internal static async Task Run()
        {
            Directory.CreateDirectory(OutputDir);
            Directory.CreateDirectory(PageDir);

            var allJobs = new List<object>();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "HojlundScraper/1.0 - Efter aftale med Daniel Egeberg (kontakt: jesper@hillgrove.dk)");

            int page = 1;
            int total = 0;
            bool isDone = false;
            while (!isDone)
            {
                Console.WriteLine($"[Side {page}] Henter data..");

                var feed = await LoadRssAsync(httpClient, page++);
                foreach (var item in feed.Items)
                {
                    var job = await ProcessJobItemAsync(httpClient, item);
                    if (job != null)
                    {
                        allJobs!.Add(job);
                    }

                    total++;
                    await Task.Delay(500);
                }

                Console.WriteLine($"Fundet {allJobs?.Count ?? 0} jobs");
                //isDone = !feed.Items.Any();
                isDone = true;
                await Task.Delay(500);
            }

            var filename    = $"jobindex_results_{DateTime.Now:yyyy-MM-dd}.json";
            var path        = Path.Combine(OutputDir, filename);
            File.WriteAllText(path, JsonSerializer.Serialize(allJobs, JsonSerializerOptions));
            Console.WriteLine($"Gemte {allJobs?.Count ?? 0} jobopslag i {path}");
        }

        private static async Task<SyndicationFeed> LoadRssAsync(HttpClient httpClient, int page)
        {
            var url = $"{RssUrl}&{PageQueryParam}{page}";
            using var stream = await httpClient.GetStreamAsync(url);
            return SyndicationFeed.Load(XmlReader.Create(stream));
        }

        private static async Task<object?> ProcessJobItemAsync(HttpClient httpClient, SyndicationItem item)
        {
            try
            {
                var summaryUrl = item.Links[0].Uri.ToString();
                var summaryHtml = await httpClient.GetStringAsync(summaryUrl);

                var doc = new HtmlDocument();
                doc.LoadHtml(summaryHtml);

                var seeJobLinkNode = doc.DocumentNode.SelectSingleNode("//a[contains(@class,'seejobdesktop') or contains(@class,'seejobmobil')]") 
                    ?? doc.DocumentNode.SelectSingleNode("//a[normalize-space(text())='Se jobbet']");
                var seeJobUrl = seeJobLinkNode?.GetAttributeValue("href", string.Empty);
                var isJobDescriptionOnJobindex = seeJobUrl?.Contains("jobindex.dk") ?? false;

                string? fullDescriptionHtml = null;
                
                if (isJobDescriptionOnJobindex)
                {
                    fullDescriptionHtml = await httpClient.GetStringAsync(seeJobUrl);

                    if (!string.IsNullOrEmpty(seeJobUrl))
                    {
                        var hash = HashUrl(seeJobUrl);
                        var filePath = Path.Combine(PageDir, $"{hash}.html");
                        File.WriteAllText(filePath, fullDescriptionHtml); 
                    }
                }

                return new
                {
                    id = item.Id,
                    titel = item.Title.Text,
                    shortDescriptionHtml = item.Summary.Text,
                    fullDescriptionHtml,
                    summaryUrl,
                    seeJobUrl,
                    isJobDescriptionOnJobindex,
                    scrapedAt = DateTime.UtcNow.ToString("O"),
                    source = "jobindex.dk",
                };
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved behandling af job: {ex.Message}");
                return null;
            }
        }

        private static object HashUrl(string url)
        {
            var hash = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(url));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()[..12];
        }
    }
}

