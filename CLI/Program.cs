using CLI;
using Google.Cloud.BigQuery.V2;

var searchQuery         = "software engineer OR udvikler OR programmør OR fullstack OR frontend OR backend OR web OR app OR database";
var sources             = new[] { "jobindex", "serpapi" };
var date                = DateTime.UtcNow;

var projectId           = "verdant-future-459722-k0";
var datasetId           = "jobdata";
var bucket              = "jobdata-pipeline";

string gcsKeyFilePath   = GetKeyFilePath();
var credential          = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(gcsKeyFilePath);
var client              = await BigQueryClient.CreateAsync(projectId, credential);


// Extract data
await Extraction.Run(searchQuery);


// Upload til GCS
await Upload.Run(credential, "data/raw", bucket, "raw");

// Load JSON filer fra GCS til BigQuery
await Load.Run(client, date, sources, gcsKeyFilePath, bucket, datasetId, projectId);

// Transform data


static string GetKeyFilePath()
{
    var gcsKeyFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DataTransfer", "Secrets", "gcs-key.json");
    if (!File.Exists(gcsKeyFilePath))
        throw new FileNotFoundException($"Filen '{gcsKeyFilePath}' blev ikke fundet. Sørg for at placere nøglen korrekt.");
    return gcsKeyFilePath;
}

#region Reset tools
//await Extraction.ExtractHistorialData();
//await Transform.FullTransformation(client, projectId, datasetId);
#endregion