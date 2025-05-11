using Newtonsoft.Json.Linq;
using SerpApi;
using System.Collections;
using System.Threading.Tasks;

namespace Scrapers
{
    internal class SerpApiScraper
    {
        private const string ApiKey = "";
        private static readonly string BaseQuery = Uri.EscapeDataString("udvikler OR software OR programmering OR developer OR programmør OR database OR Devops");
        private static readonly string BaseUrl = $"https://serpapi.com/search.json?engine=google_jobs&q={BaseQuery}&location=Denmark&google_domain=google.dk&gl=dk&hl=da&api_key={ApiKey}";

        public static async Task Run()
         {
            var allResults = new List<JObject>();
            string nextUrl = BaseUrl;
            int page = 1;

            using var client = new HttpClient();

            while (!string.IsNullOrEmpty(nextUrl))
            {
                try
                {
                    var response = client.GetStringAsync(nextUrl).Result;
                    var json = JObject.Parse(response);

                    var jobs = (JArray)json["jobs_results"];
                    Console.WriteLine($"[Page {page++}] Found {jobs?.Count ?? 0} jobs");

                    if (jobs != null)
                    {
                        foreach (JObject job in jobs)
                        {
                            allResults.Add(job);
                        }
                    }

                    nextUrl = json["serpapi_pagination"]?["next"]?.ToString();
                    Console.WriteLine($"Next URL: {(nextUrl != null ? "Found" : "Not Found")}");

                    await Task.Delay(2000); // Delay to avoid hitting the API too fast
                }

                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"❌ HTTP fejl ved side {page}: {ex.Message}");
                    break;
                }
            }

            Console.WriteLine($"[Done] Found {allResults.Count} jobs in total.");
        }
    }
}
