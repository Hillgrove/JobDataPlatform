CREATE OR REPLACE TABLE jobdata.job_technologies AS
WITH flattened AS (
  
  SELECT job_id, LOWER(skill) AS name, 'skill' AS type
  FROM jobdata.enriched_job_listings, UNNEST(skills) AS skill
  
  UNION ALL
  
  SELECT job_id, LOWER(fw), 'framework'
  FROM jobdata.enriched_job_listings, UNNEST(frameworks) AS fw
  
  UNION ALL
  
  SELECT job_id, LOWER(lang), 'language'
  FROM jobdata.enriched_job_listings, UNNEST(programming_languages) AS lang

  UNION ALL

  SELECT job_id, LOWER(db), 'database'
  FROM jobdata.enriched_job_listings, UNNEST(databases) AS db
)
SELECT
  f.job_id,
  t.tech_id
FROM flattened f
JOIN jobdata.technologies t
  ON f.name = t.name AND f.type = t.type;
