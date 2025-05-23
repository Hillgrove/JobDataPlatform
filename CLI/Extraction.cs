using Extract;

namespace CLI
{
    public static class Extraction
    {
        public static async Task Run(string searchQuery)
        {
            Console.WriteLine("Kører extraction...");
            await Jobindex.Extract();
            await SerpApi.Extract(searchQuery);
            Console.WriteLine("Extraction færdig.\n");
        }

        public static async Task ExtractHistorialData()
        {
            Console.WriteLine("Kører extraction af historiske data...");
            await SerpApiHistoricExtract.RunFullHistoricalScrape();
            Console.WriteLine("Extraction færdig.\n");
        }
    }
}
