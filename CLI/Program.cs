using Extract;
using Load;

//LocationParser.Load("data/postnumre.json");
//ProgrammingLanguageParser.Load("data/programming_languages.json");

await Jobindex.Extract();

string searchQuery = "software engineer OR udvikler OR programmør OR fullstack OR frontend OR backend OR web OR app OR database";
await SerpApi.Extract(searchQuery);

await GcsUploader.UploadAllFilesAsync("data/raw", "jobdata-pipeline", "raw");