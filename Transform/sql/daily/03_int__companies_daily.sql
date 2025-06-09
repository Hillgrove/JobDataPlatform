-- Inkrementel opdatering af jobdata.companies
-- Denne query finder nye virksomheder i enriched_job_listings
-- og indsætter dem i jobdata.companies med et nyt, kontinuerligt ID

INSERT INTO jobdata.companies (company_id, company_name)
SELECT
  -- Tildeler nye firmaer fortløbende ID'er efter højeste eksisterende (inkrementelt)
  ROW_NUMBER() OVER () + IFNULL((
    SELECT MAX(company_id) FROM jobdata.companies
  ), 0) AS company_id,
  company_name
FROM (

  -- Find alle unikke virksomhedsnavne fra enriched-tabellen
  SELECT DISTINCT TRIM(LOWER(company_name)) AS company_name
  FROM jobdata.enriched_job_listings
  WHERE company_name IS NOT NULL AND company_name != ''
)

-- Udeluk dem vi allerede har i companies-tabellen
WHERE company_name NOT IN (
  SELECT company_name FROM jobdata.companies
);
