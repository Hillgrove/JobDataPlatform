namespace Transform
{
    public class DailyTransform
    {
        private static readonly string SqlDir = "sql/daily";
        
        private static readonly string[] Files = new[]
        {
        "03_int__enriched_job_listings_daily.sql",
        "03_int__companies_daily.sql",
        "03_int__domains_daily.sql",
        "03_int__technologies_daily.sql",
        "03_int__job_details_domains_daily.sql",
        "03_int__job_technologies_daily.sql"
    };
    }
}
