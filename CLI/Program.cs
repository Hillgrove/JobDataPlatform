using CLI;


var searchQuery     = "software engineer OR udvikler OR programmør OR fullstack OR frontend OR backend OR web OR app OR database";
//var sources         = new[] { "jobindex", "serpapi" };
var sources         = new[] { "serpapi" };
var date            = DateTime.UtcNow;

//LocationParser.Load("data/postnumre.json");
//ProgrammingLanguageParser.Load("data/programming_languages.json");

// Extract data
await Extraction.Run(searchQuery);

// Upload til GCS
await Upload.Run("data/raw", bucketName: "jobdata-pipeline", gcsPrefix: "raw");

// Load JSON filer fra GCS til BigQuery
await Load.Run(date, sources);

// Transform data