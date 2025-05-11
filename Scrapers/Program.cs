using Scrapers;


LocationParser.Load("data/postnumre.json");
ProgrammingLanguageParser.Load("data/programming_languages.json");

//await JobIndexScraper.Run();
SerpApiScraperV2.Run();   