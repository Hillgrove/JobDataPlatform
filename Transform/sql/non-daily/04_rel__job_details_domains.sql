CREATE OR REPLACE TABLE jobdata.job_details_domains AS
SELECT
  j.job_id,
  d.domain_id
FROM jobdata.enriched_job_listings j,
UNNEST(j.job_details_domain) AS domain_str
JOIN jobdata.domains d
  ON LOWER(domain_str) = d.domain;
