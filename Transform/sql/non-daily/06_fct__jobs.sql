CREATE OR REPLACE VIEW jobdata.jobs AS
WITH enriched AS (
  SELECT
    j.job_id,
    j.title,
    c.company_id,
    j.location,
    j.level_name,
    j.role_name,
    j.remote_friendly,
    j.scraped_from,
    j.scraped_at
    
  FROM jobdata.enriched_job_listings j
  JOIN jobdata.companies c
    ON j.company_name = c.company_name
)


SELECT
  e.job_id,
  e.title,
  e.company_id,
  e.location,
  l.level_id,
  r.role_id,
  e.remote_friendly,
  e.scraped_from,
  e.scraped_at

FROM enriched e
LEFT JOIN jobdata.roles r
  ON e.role_name = r.role_name
LEFT JOIN jobdata.levels l
  ON e.level_name = l.level_name;
