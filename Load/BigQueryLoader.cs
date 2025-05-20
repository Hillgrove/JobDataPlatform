using Google.Cloud.BigQuery.V2;

namespace DataTransfer
{
    public static class BigQueryLoader
    {
        public static async Task LoadAsync(
            string gcsUri,
            string datasetId,
            string tableId,
            string projectId,
            string gcsKeyFilePath,
            string partitionField = "scrapedAt",
            bool autodetect = true)
        {
            var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(gcsKeyFilePath);
            var client = await BigQueryClient.CreateAsync(projectId, credential);

            // Destinationstabel i BigQuery
            var tableRef = client.GetTableReference(projectId, datasetId, tableId);

            var jobOptions = new CreateLoadJobOptions
            {
                SourceFormat = FileFormat.NewlineDelimitedJson,
                WriteDisposition = WriteDisposition.WriteAppend,
                Autodetect = autodetect,
                TimePartitioning = new Google.Apis.Bigquery.v2.Data.TimePartitioning
                {
                    Type = "DAY",
                    Field = partitionField
                }
            };

            try
            {
                // Start load jobbet fra GCS til BigQuery
                var job = await client.CreateLoadJobAsync(gcsUri, tableRef, schema: null, options: jobOptions);
                Console.WriteLine($"Job started: {job.Reference.JobId}");

                // Vent på at jobbet er færdigt
                job = job.PollUntilCompleted();

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

                Console.WriteLine($"BigQuery load job udført. Rækker indlæst: {job.Statistics.Load.OutputRows}");
            }

            catch (Google.GoogleApiException ex)
            {
                Console.WriteLine("BigQuery API fejl:");
                Console.WriteLine(ex.Message);

                if (ex.Error != null)
                {
                    Console.WriteLine($"Fejltype: {ex.Error.Code} - {ex.Error.Message}");
                    if (ex.Error.Errors != null)
                    {
                        foreach (var error in ex.Error.Errors)
                        {
                            Console.WriteLine($" - {error.Message}");
                        }
                    }
                }
            }
            
        }
    }
}
