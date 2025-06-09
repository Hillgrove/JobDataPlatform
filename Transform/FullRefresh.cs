
using Google.Cloud.BigQuery.V2;


namespace Transform
{
    public class FullRefresh
    {
        private static readonly string projectId = "verdant-future-459722-k0";
        private static readonly string datasetId = "jobdata";
        private static readonly string _sqlDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Transform", "sql", "non-daily");

        private static readonly string[] _files =
        [
            // DIMENSIONSTABELLER – bruges direkte i enriched_job_listings
            "01_dim__roles.sql",
            "01_dim__levels.sql",
            "01_dim__skills.sql",
            "01_dim__programming_languages.sql",
            "01_dim__databases_and_storage.sql",
            "01_dim__web_frameworks_and_technologies.sql",

            // ENRICHED DATASET – afhænger af raw + dimensionstabeller
            "02_enriched__job_listings.sql",

            // INTERMEDIATE TABELLER – afhænger af enriched
            "03_dim__companies.sql",
            "03_dim__domains.sql",
            "03_dim__technologies.sql",

            // lav godt navn
            "04_rel__job_details_domains.sql",
            "04_rel__job_technologies.sql",

            // STOPWORDS
            "05_dim__stop_words_manual.sql",
            "05_dim__stop_words_system_generated.sql",
            "05_dim__stop_words.sql",

            // FACT TABLE – afhænger af enriched + companies + dim
            "06_fct__jobs.sql",

            // VIEW TABLES – afhænger af faktatabeller
            "07_rep__jobs_exploded.sql",
            "07_rep__jobs_flattened.sql"
        ];

        public static async Task RunAsync()
        {
            var gcsKeyFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DataTransfer", "Secrets", "gcs-key.json");
            if (!File.Exists(gcsKeyFilePath))
                throw new FileNotFoundException($"Filen '{gcsKeyFilePath}' blev ikke fundet. Sørg for at placere nøglen korrekt.");

            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(gcsKeyFilePath);
            var client = await BigQueryClient.CreateAsync(projectId, credential);
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

                //await client.ExecuteQueryAsync(sql, parameters: null, queryOptions);

                try
                {
                    var job = await client.ExecuteQueryAsync(sql, parameters: null, queryOptions);
                }
                catch (Google.GoogleApiException ex)
                {
                    Console.WriteLine($"Fejl i fil: {file}");
                    Console.WriteLine(ex.Message);
                    if (ex.Error != null)
                    {
                        foreach (var error in ex.Error.Errors)
                        {
                            Console.WriteLine($"BigQuery-fejl: {error.Message}");
                        }
                    }
                    throw;
                }


                Console.WriteLine($"Færdig med: {file}\n");
            }
        }
    }
}
