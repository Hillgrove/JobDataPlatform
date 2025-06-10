using Google.Cloud.BigQuery.V2;

namespace Transform
{
    public class DailyTransform
    {
        private static readonly string _sqlDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Transform", "sql", "daily");

        private static readonly string[] _files =
        [
            // ENRICHED DATASET
            "02_enriched__job_listings_daily.sql",
            
            // INTERMEDIATE TABELLER – afhænger af enriched
            "03_dim__companies_daily.sql",
            "03_dim__domains_daily.sql",
            "03_dim__technologies_daily.sql",
            
            // RELATIONS TABELLER – N:M-forbindelser mellem jobs og domæner/teknologier
            "04_rel__job_details_domains_daily.sql",
            "04_rel__job_technologies_daily.sql"
        ];

        public static async Task RunAsync(BigQueryClient client, string projectId, string datasetId)
        {
            var datasetRef = client.GetDatasetReference(projectId, datasetId);
            
            foreach (var file in _files)
            {
                var path = Path.Combine(_sqlDir, file);
                var sql = await File.ReadAllTextAsync(path);
                
                Console.WriteLine($"Kører: {file}");
                
                var queryOptions = new QueryOptions
                {
                    DefaultDataset = datasetRef
                };

                var job = await client.CreateQueryJobAsync(sql, parameters: null, queryOptions);
                job = await job.PollUntilCompletedAsync();
                if (job.Status.State != "DONE")
                {
                    Console.WriteLine($"Job Status: {job.Status.State}");
                    Console.WriteLine($"Fejl: {job.Status.ErrorResult?.Message}");
                    if (job.Status.Errors != null)
                    {
                        foreach (var err in job.Status.Errors)
                        {
                            Console.WriteLine($" - {err.Message}");
                        }
                    }
                    throw new Exception("BigQuery Job fejlede.");
                }

                Console.WriteLine($"Færdig: {file}");
            }
        }
    }
}
