using Google.Cloud.BigQuery.V2;
using Transform;

namespace CLI
{
    public static class Transformation
    {
        public static async Task Run(BigQueryClient client, string projectId, string datasetId)
        {
            Console.WriteLine("Kører daglig transformation...");
            await DailyTransform.RunAsync(client, projectId, datasetId);
            Console.WriteLine("Daglig transformation færdig.\n");
        }

        public static async Task FullTransformation(BigQueryClient client, string projectId, string datasetId)
        {
            Console.WriteLine("Kører fuld transformation...");
            await FullTransform.RunAsync(client, projectId, datasetId);
            Console.WriteLine("Fuld transformation færdig.\n");
        }
    }
}
