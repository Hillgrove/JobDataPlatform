CREATE OR REPLACE TABLE jobdata.domains AS
SELECT
  ROW_NUMBER() OVER (ORDER BY domain) AS domain_id,
  domain
FROM (
  SELECT DISTINCT LOWER(domain) AS domain
  FROM jobdata.enriched_job_listings, UNNEST(job_details_domain) AS domain
);