using HtmlAgilityPack;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Xml;


namespace Extract
{
    public static class Jobindex
    {
        private const string RssUrl             = "https://www.jobindex.dk/jobsoegning.rss?geoareaid=1221&subid=1&jobage=1";
        private const string PageQueryParam     = "page=";
        private const string OutputDir          = "data/raw";
        private static readonly HttpClient httpClient;

        static Jobindex()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "HojlundScraper/1.0 - Efter aftale med Daniel Egeberg (kontakt: jesper@hillgrove.dk)");
        }

        public static async Task Extract()
        {
            var allJobs = await ScrapeJobs();
            await WriteJobsToJson(allJobs);
        }
        private static async Task<List<object>?> ScrapeJobs()
        {
            Console.WriteLine("Scraper Jobindex:");
            
            var allJobs = new List<object>();
            int page = 1;
            int total = 0;
            bool isDone = false;

            while (!isDone)
            {
                Console.WriteLine($" [Side {page}] Henter data..");

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

                Console.WriteLine($"  Fundet {allJobs?.Count ?? 0} jobs");
                isDone = !feed.Items.Any();

                await Task.Delay(500);
            }

            Console.WriteLine($" Gennemførte {page - 1} side(r) med {allJobs?.Count} jobopslag.");
            return allJobs;
        }

        private static async Task WriteJobsToJson(List<object>? allJobs)
        {            
            if (allJobs == null || allJobs.Count == 0)
            {
                Console.WriteLine("Ingen jobopslag gemt.\n");
                return;
            }

            Directory.CreateDirectory(OutputDir);
            var filename = $"jobindex_results_{DateTime.Now:yyyy-MM-dd}.ndjson";
            var path = Path.Combine(OutputDir, filename);

            await using var writer = File.CreateText(path);
            
            foreach (var job in allJobs)
            {
                var line = JsonSerializer.Serialize(job);
                await writer.WriteLineAsync(line);
            }

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
                    var fullHtml = await httpClient.GetStringAsync(seeJobUrl);
                    var fullDoc = new HtmlDocument();
                    fullDoc.LoadHtml(fullHtml);

                    var articleNode = fullDoc.DocumentNode.SelectSingleNode("//article[contains(@class,'jobtext-jobad')]");
                    fullDescriptionHtml = articleNode?.OuterHtml;

                    // Saves the full job description to a file
                    //if (!string.IsNullOrEmpty(seeJobUrl))
                    //{
                    //    var hash = HashUrl(seeJobUrl);
                    //    var filePath = Path.Combine(PageDir, $"{hash}.html");
                    //    File.WriteAllText(filePath, fullDescriptionHtml); 
                    //}
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
                    scrapedAt = DateTime.UtcNow.ToString("yyyy-MM-dd"),
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

