using Newtonsoft.Json.Linq;
using SerpApi;
using System.Collections;

namespace Scrapers
{
    internal class SerpApiScraperV2
    {
        private const string ApiKey = "";

        public static async Task Run()
        {
            var parameters = new Hashtable
            {
                { "engine", "google_jobs" },
                { "google_domain", "google.dk" },
                { "q", "udvikler OR developer OR softwareudvikler OR programmør OR fullstack OR frontend OR backend OR webudvikler OR appudvikler OR \"it konsulent\" OR \"software engineer\" OR database OR software" },
                { "hl", "da" },
                { "gl", "dk" },
                { "location", "Denmark" }
            };

            try
            {
                var search = new GoogleSearch(parameters, ApiKey);
                JObject data = search.GetJson();
                JArray results = (JArray)data["jobs_results"];
                Console.WriteLine($"Found {results?.Count ?? 0} jobs");

                foreach (var job in results)
                {
                    var description = job["description"]?.ToString() ?? "";
                    var languages = ProgrammingLanguageParser.Extract(description);

                    Console.WriteLine($"Languages: {string.Join(", ", languages)}");
                }

                Console.WriteLine("Jobs scrabed");
            }
            
            catch (SerpApiSearchException ex)
            {
                Console.WriteLine($"API fejl: {ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Uventet fejl: {ex.Message}");
            }
        }
    }
}
