using DataTransfer;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;

namespace CLI
{
    public static class Load
    {
        public static async Task RunAsync(
            BigQueryClient client,
            DateTime date, 
            string[] sources, 
            string gcsKeyFilePath, 
            string bucket, 
            string datasetId, 
            string projectId)
        {
            var storage = await StorageClient.CreateAsync(Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(gcsKeyFilePath));

            foreach (var source in sources)
            {
                string fileName     = $"{source}_results_{date:yyyy-MM-dd}.ndjson";
                string gcsPath      = $"raw/{fileName}";
                string gcsUri       = $"gs://{bucket}/{gcsPath}";
                string tableId      = $"raw_{source}";

                bool exists;
                try { await storage.GetObjectAsync(bucket, gcsPath); exists = true; }
                catch (Google.GoogleApiException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound) { exists = false; }

                if (exists)
                {
                    Console.WriteLine($"Loader {fileName} → BigQuery ({tableId})");
                    await BigQueryLoader.LoadAsync(client, gcsUri, datasetId, tableId, projectId);
                }
                else
                {
                    Console.WriteLine($"Fil ikke fundet i GCS: {fileName}");
                }
            }
        }
    }
}
