using Newtonsoft.Json.Linq;
using Scrapers.Parser;
using System.Net;
using System.Text.Json;


namespace Scrapers
{
    internal class SerpApiScraperWrong
    {
        private const string ApiKey                 = "7fa1c9ffbc7c9379d5f45d8d3a6535535b592cf9b5925c2b555da4ee2e197017";
        private static readonly string BaseQuery    = WebUtility.UrlEncode("udvikler OR developer OR softwareudvikler OR programmør OR fullstack OR frontend OR backend OR webudvikler OR \"app udvikler\" OR \"it konsulent\" OR \"software engineer\" OR database OR software");
        private static readonly string BaseUrl      = $"https://serpapi.com/search.json?engine=google_jobs&q={BaseQuery}&location=Denmark&google_domain=google.dk&gl=dk&hl=da&api_key={ApiKey}";

        public static async Task Run()
         {
            var allJobs = new List<JobEntry>();
            string? nextUrl = BaseUrl;
            int page = 1;

            using var client = new HttpClient();

            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    if (!nextUrl.Contains("api_key="))
                        nextUrl += $"&api_key={ApiKey}";

                    var response = await client.GetStringAsync(nextUrl);
                    var json = JObject.Parse(response);

                    var jobs = (JArray)json["jobs_results"];
                    Console.WriteLine($"[Page {page++}] Found {jobs?.Count ?? 0} jobs");

                    if (jobs != null)
                    {
                        foreach (JObject job in jobs)
                        {
                            string? jobId                       = job["job_id"]?.ToString();  
                            string? title                       = job["title"]?.ToString();
                            string? company                     = job["company_name"]?.ToString();
                            string? locationRaw                 = job["location"]?.ToString();
                            string? via                         = job["via"]?.ToString();
                            string? postedAt                    = job["detected_extensions"]?["posted_at"]?.ToString();
                            string? scheduleType                = job["detected_extensions"]?["schedule_type"]?.ToString();
                            string? description                 = job["description"]?.ToString();

                            string? postalCode                  = locationRaw != null ? LocationParser.Extract(locationRaw) : null;
                            List<string>? programmingLanguages  = description != null ? ProgrammingLanguageParser.Extract(description) : null;

                            allJobs.Add(new JobEntry
                            {
                                jobId = jobId,
                                Title = title,
                                Company = company,
                                LocationRaw = locationRaw,
                                PostalCode = postalCode,
                                Via = via,
                                PostedAt = postedAt,
                                ScheduleType = scheduleType,
                                Description = description,
                                ProgrammingLanguages = programmingLanguages
                            });

                        }
                    }

                    nextUrl = json["serpapi_pagination"]?["next"]?.ToString();
                    Console.WriteLine($"Next URL: {(nextUrl != null ? "Found" : "Not Found")}");

                    await Task.Delay(500); // Delay to avoid hitting the API too fast
                }

                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP fejl ved side {page}: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine($"[Done] Found {allJobs.Count} jobs in total.");

            var jsonOutput = JsonSerializer.Serialize(allJobs, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync("data/serpapi_jobs.json", jsonOutput);
            Console.WriteLine("Saved results to data/serpapi_jobs.json");
        }
    }

    internal class JobEntry
    {
        public string? jobId { get; set; }
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? LocationRaw { get; set; }
        public string? PostalCode { get; set; }
        public string? Via { get; set; }
        public string? PostedAt { get; set; }
        public string? ScheduleType { get; set; }
        public string? Description { get; set; }
        public List<string>? ProgrammingLanguages { get; set; } = new();
    }
}
