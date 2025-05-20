using Newtonsoft.Json.Linq;
using System.Web;

namespace Extract
{
    public static class SerpApiHistoricExtract
    {
        private const string OutputFile = "data/raw/serpapi_results_historic.ndjson";
        private static readonly HttpClient client = new();

        public static async Task RunFullHistoricalScrape()
        {
            Console.WriteLine("Kører historisk SerpApi scrape...");

            var apiKey = Environment.GetEnvironmentVariable("SERP_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("SERP_API_KEY ikke sat i miljøvariabler.");

            var queryParams = new Dictionary<string, string>
            {
                { "engine", "google_jobs" },
                { "q", "software engineer OR udvikler OR programmør OR fullstack OR frontend OR backend OR web OR app OR database" },
                { "location", "Denmark" },
                { "google_domain", "google.dk" },
                { "gl", "dk" },
                { "hl", "da" },
                { "api_key", apiKey },
                { "num", "100" }
            };

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            var baseUrl = $"https://serpapi.com/search.json?{queryString}";

            var allJobs = await FetchJobPages(baseUrl, apiKey);
            await WriteJobsToJson(allJobs);

            Console.WriteLine($"Gemte {allJobs.Count} jobopslag til {OutputFile}");
        }

        private static async Task<JArray> FetchJobPages(string startUrl, string apiKey)
        {
            var allJobs = new JArray();
            var nextUrl = startUrl;
            int page = 1;

            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    Console.WriteLine($" [Side {page++}] Henter data...");

                    if (!nextUrl.Contains("api_key="))
                        nextUrl += $"&api_key={apiKey}";

                    var response = await client.GetStringAsync(nextUrl);
                    var json = JObject.Parse(response);

                    if (json["jobs_results"] is JArray jobs) allJobs.Merge(jobs);

                    nextUrl = json["serpapi_pagination"]?["next"]?.ToString();
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fejl: {ex.Message}");
                    break;
                }
            }

            return allJobs;
        }

        private static async Task WriteJobsToJson(JArray allJobs)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputFile)!);

            await using var writer = File.CreateText(OutputFile);
            var scrapedAt = DateTime.UtcNow.ToString("yyyy-MM-dd");

            foreach (var job in allJobs)
            {
                if (job is JObject obj)
                {
                    if (obj["detected_extensions"] is JObject ext)
                    {
                        obj["detected_extensions_posted_at"] = ext["posted_at"];
                        obj["detected_extensions_schedule_type"] = ext["schedule_type"];
                        obj.Remove("detected_extensions");
                    }

                    obj.Property("thumbnail")?.Remove();
                    obj["job_id"] = $"id_{obj["job_id"]}";
                    obj["scrapedAt"] = scrapedAt;
                    obj["source"] = "serpapi.com";
                }

                var jsonLine = Newtonsoft.Json.JsonConvert.SerializeObject(job, Newtonsoft.Json.Formatting.None);
                await writer.WriteLineAsync(jsonLine);
            }
        }
    }
}
