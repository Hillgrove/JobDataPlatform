CREATE OR REPLACE TABLE jobdata.companies AS
WITH deduplicated AS (
  SELECT
    TRIM(LOWER(company_name)) AS company_name,
    COUNT(*) AS postings
  FROM jobdata.enriched_job_listings
  WHERE company_name IS NOT NULL AND company_name != ''
  GROUP BY company_name
),
ranked AS (
  SELECT *,
    ROW_NUMBER() OVER (PARTITION BY company_name ORDER BY postings DESC) AS row_num
  FROM deduplicated
)
SELECT
  ROW_NUMBER() OVER (ORDER BY company_name) AS company_id,
  company_name
FROM ranked
WHERE row_num = 1;
