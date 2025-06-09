-- Inkrementel opdatering af jobdata.domains
-- Denne query finder nye domæner i enriched_job_listings
-- og indsætter dem i jobdata.domains med et nyt, kontinuerligt ID

INSERT INTO jobdata.domains (domain_id, domain)
SELECT
  -- Tildeler nye domæner fortløbende ID'er efter højeste eksisterende (inkrementelt)
  ROW_NUMBER() OVER () + IFNULL((
    SELECT MAX(domain_id) FROM jobdata.domains
  ), 0) AS domain_id,
  domain
FROM (
  -- Find alle unikke domæner i job_details_domain-arrayet
  SELECT DISTINCT LOWER(domain) AS domain
  FROM jobdata.enriched_job_listings, UNNEST(job_details_domain) AS domain
)

-- Udeluk dem vi allerede har i domains-tabellen
WHERE domain NOT IN (
  SELECT domain FROM jobdata.domains
);
