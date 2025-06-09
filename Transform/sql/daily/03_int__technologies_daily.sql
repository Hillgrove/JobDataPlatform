-- Inkrementel opdatering af jobdata.job_technologies
-- Denne query finder nye (job_id, tech_id) relationer fra enriched_job_listings
-- og indsætter kun de manglende forbindelser i jobdata.job_technologies

INSERT INTO jobdata.job_technologies (job_id, tech_id)
SELECT
  f.job_id,
  t.tech_id
FROM (
  -- Flad struktur: job_id × teknologi × type
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
) f
JOIN jobdata.technologies t
  ON f.name = t.name AND f.type = t.type

-- Undgå at tilføje duplikate forbindelser
WHERE NOT EXISTS (
  SELECT 1
  FROM jobdata.job_technologies jt
  WHERE jt.job_id = f.job_id AND jt.tech_id = t.tech_id
);
