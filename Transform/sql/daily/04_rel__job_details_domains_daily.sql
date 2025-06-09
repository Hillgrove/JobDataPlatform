-- Inkrementel opdatering af jobdata.job_details_domains
-- Finder nye job_id + domain_id kombinationer og indsætter dem

INSERT INTO jobdata.job_details_domains (job_id, domain_id)
SELECT
  j.job_id,
  d.domain_id
FROM jobdata.enriched_job_listings j,
UNNEST(j.job_details_domain) AS domain_str
JOIN jobdata.domains d
  ON LOWER(domain_str) = d.domain

-- Udeluk allerede eksisterende relationer
WHERE NOT EXISTS (
  SELECT 1
  FROM jobdata.job_details_domains existing
  WHERE existing.job_id = j.job_id
    AND existing.domain_id = d.domain_id
);
