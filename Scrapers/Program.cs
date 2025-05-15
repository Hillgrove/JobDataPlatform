using Scrapers;
using Scrapers.Parser;


//LocationParser.Load("data/postnumre.json");
//ProgrammingLanguageParser.Load("data/programming_languages.json");

await JobIndexScraper.Run();

//string searchQuery = "udvikler OR developer OR softwareudvikler OR programmør OR fullstack OR frontend OR backend OR webudvikler OR \"app udvikler\" OR \"it konsulent\" OR \"software engineer\" OR database OR software";
//await RawDataExtractor_SerpAPI.Run(searchQuery);