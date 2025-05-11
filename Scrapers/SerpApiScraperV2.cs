using Newtonsoft.Json.Linq;
using SerpApi;
using System.Collections;

namespace Scrapers
{
    internal class SerpApiScraperV2
    {
        private const string ApiKey = "b10dfef248c986635836cb66e220ec8d6d44dc2f05da64d0dbb8aac534e95d89";

        public static void Run()
        {
            var parameters = new Hashtable
            {
                { "engine", "google_jobs" },
                { "google_domain", "google.dk" },
                { "q", "udvikler OR software OR programmering OR developer OR programmør OR database OR Devops" },
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
