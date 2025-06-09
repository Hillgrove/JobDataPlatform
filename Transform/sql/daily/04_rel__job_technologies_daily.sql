-- Inkrementel opdatering af jobdata.job_technologies
-- Finder nye job ↔ teknologi-relationer fra enriched_job_listings
-- og indsætter kun dem, der ikke allerede findes i job_technologies

INSERT INTO jobdata.job_technologies (job_id, tech_id)
SELECT
  f.job_id,
  t.tech_id
FROM (
  -- Udpak alle teknologier (skills, frameworks, languages) med type
  
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

) AS f
JOIN jobdata.technologies t
  ON f.name = t.name AND f.type = t.type

-- Udeluk allerede eksisterende relationer
WHERE NOT EXISTS (
  SELECT 1
  FROM jobdata.job_technologies jt
  WHERE jt.job_id = f.job_id AND jt.tech_id = t.tech_id
);
