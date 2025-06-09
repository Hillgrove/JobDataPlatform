CREATE OR REPLACE TABLE jobdata.technologies AS
WITH all_techs AS (
  
  SELECT DISTINCT LOWER(skill) AS name, 'skill' AS type
  FROM jobdata.enriched_job_listings, UNNEST(skills) AS skill
  
  UNION DISTINCT
  
  SELECT LOWER(fw), 'framework' 
  FROM jobdata.enriched_job_listings, UNNEST(frameworks) AS fw
  
  UNION DISTINCT
  
  SELECT LOWER(lang), 'language' 
  FROM jobdata.enriched_job_listings, UNNEST(programming_languages) AS lang

  UNION DISTINCT

  SELECT DISTINCT LOWER(db), 'database'
  FROM jobdata.enriched_job_listings, UNNEST(databases) AS db
)
SELECT
  ROW_NUMBER() OVER (ORDER BY type, name) AS tech_id,
  name,
  type
FROM all_techs;
