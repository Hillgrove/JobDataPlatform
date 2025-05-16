using Extract;
using Load;


//LocationParser.Load("data/postnumre.json");
//ProgrammingLanguageParser.Load("data/programming_languages.json");

//await Jobindex.Extract();

//string searchQuery = "udvikler OR developer OR softwareudvikler OR programmør OR fullstack OR frontend OR backend OR webudvikler OR \"app udvikler\" OR \"it konsulent\" OR \"software engineer\" OR database OR software";
//await SerpApi.Extract(searchQuery);

await GcsUploader.UploadAllFilesAsync("data/raw", "jobdata-pipeline", "raw");