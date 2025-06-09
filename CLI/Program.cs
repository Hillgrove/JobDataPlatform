using Google.Cloud.BigQuery.V2;

var projectId = "verdant-future-459722-k0";
var datasetId = "jobdata";

var gcsKeyFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DataTransfer", "Secrets", "gcs-key.json");
if (!File.Exists(gcsKeyFilePath))
    throw new FileNotFoundException($"Filen '{gcsKeyFilePath}' blev ikke fundet. Sørg for at placere nøglen korrekt.");

var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(gcsKeyFilePath);
var client = await BigQueryClient.CreateAsync(projectId, credential);

var searchQuery = "software engineer OR udvikler OR programmør OR fullstack OR frontend OR backend OR web OR app OR database";
var sources = new[] { "jobindex", "serpapi" };
var date = DateTime.UtcNow;

// Extract data
//await Extraction.Run(searchQuery);


// Upload til GCS
//await Upload.Run("data/raw", bucketName: "jobdata-pipeline", gcsPrefix: "raw");

// Load JSON filer fra GCS til BigQuery
//await Load.Run(date, sources);

// Transform data



#region Helpers
//await Extraction.ExtractHistorialData();
//await Transform.FullTransformation(client, projectId, datasetId);
#endregion