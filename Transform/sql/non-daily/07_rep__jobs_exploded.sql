CREATE OR REPLACE VIEW jobdata.jobs_exploded AS
SELECT
  j.job_id,
  j.title,
  c.company_name,
  j.location,
  l.level_name,
  r.role_name,
  j.remote_friendly,
  j.scraped_from,
  j.scraped_at,
  LOWER(t.name) AS technology,
  t.type AS technology_type

FROM jobdata.enriched_job_listings j

-- Join til dim tables
LEFT JOIN jobdata.roles r ON j.role_name = r.role_name
LEFT JOIN jobdata.levels l ON j.level_name = l.level_name
LEFT JOIN jobdata.companies c ON j.company_name = c.company_name

-- Join til junction og technologies
LEFT JOIN jobdata.job_technologies jt ON j.job_id = jt.job_id
LEFT JOIN jobdata.technologies t ON jt.tech_id = t.tech_id
