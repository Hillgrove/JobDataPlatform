CREATE OR REPLACE VIEW jobdata.jobs_flattened AS
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

  ARRAY_AGG(DISTINCT t.name ORDER BY t.name) AS technologies

FROM jobdata.jobs j
JOIN jobdata.companies c                ON j.company_id = c.company_id
LEFT JOIN jobdata.levels l              ON j.level_id = l.level_id
LEFT JOIN jobdata.roles r               ON j.role_id = r.role_id
LEFT JOIN jobdata.job_technologies jt   ON j.job_id = jt.job_id
LEFT JOIN jobdata.technologies t        ON jt.tech_id = t.tech_id

GROUP BY
  j.job_id,
  j.title,
  c.company_name,
  j.location,
  l.level_name,
  r.role_name,
  j.remote_friendly,
  j.scraped_from,
  j.scraped_at;
