using Newtonsoft.Json.Linq;
using SerpApi;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Scrapers
{
    internal class SerpApiScraper
    {
        private const string ApiKey = "b10dfef248c986635836cb66e220ec8d6d44dc2f05da64d0dbb8aac534e95d89";
        //private static readonly string BaseQuery = Uri.EscapeDataString("udvikler OR developer OR softwareudvikler OR programmør OR fullstack OR frontend OR backend OR webudvikler OR \"app udvikler\" OR \"it konsulent\" OR \"software engineer\" OR database OR software");
        private static readonly string BaseQuery = WebUtility.UrlEncode("udvikler OR developer OR softwareudvikler OR programmør OR fullstack OR frontend OR backend OR webudvikler OR \"app udvikler\" OR \"it konsulent\" OR \"software engineer\" OR database OR software");
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
