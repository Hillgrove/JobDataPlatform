-- Tabel indeholder det færdigrensede og enriched datasæt, partitioneret på scraped_at (dagligt)

-- FJERN _TEST NÅR FÆRDIG

CREATE OR REPLACE TABLE jobdata.enriched_job_listings
PARTITION BY scraped_at AS

WITH merged AS (
  -- Trin 1: ensret og saml data fra både Jobindex og SerpApi med samme struktur og kolonner
  SELECT
    job_id,

    LOWER(title) AS title,

    LOWER(company_name) AS company_name,

    CASE
      WHEN location IS NULL OR TRIM(location) = '' THEN 'unknown'
      ELSE LOWER(location)
    END AS location,

    LOWER(scraped_from) AS scraped_from,

    DATE(scraped_at) AS scraped_at,

    LOWER(description) AS description,

    LOWER(job_listing_domain) AS job_listing_domain,

    -- Omskriv liste af domæner til små bogstaver
    ARRAY(
      SELECT LOWER(domain)
      FROM UNNEST(job_details_domain) AS domain
    ) AS job_details_domain,

    LOWER(job_summary_url) AS job_summary_url,

    LOWER(job_description_url) AS job_description_url

  FROM (
    -- Data fra Jobindex
    SELECT
      id AS job_id,
      REGEXP_REPLACE(titel, r',\s*[^,]+$', '') AS title, -- Rensning af titler for firmanavne (de er oftest efter et komma)
      TRIM(REGEXP_EXTRACT(titel, r',\s*([^,]+)$')) AS company_name,
      REGEXP_EXTRACT(shortDescriptionHtml, r'<span[^>]*class="jix_robotjob--area"[^>]*>([^<]+)</span>') AS location,
      source AS scraped_from,
      scrapedAt AS scraped_at,

      -- Fjern HTML-tags og trim jobbeskrivelsen
      TRIM(REGEXP_REPLACE(
        REGEXP_REPLACE(
          COALESCE(fullDescriptionHtml, shortDescriptionHtml),
          r'<[^>]+>',
          ' '
        ),
        r'\s+',
        ' '
      )) AS description,

      'jobindex.dk' AS job_listing_domain,
      [REGEXP_EXTRACT(seeJobUrl, r"https?://([^/]+)")] AS job_details_domain,
      summaryUrl AS job_summary_url,
      seeJobUrl AS job_description_url

    FROM jobdata.raw_jobindex

    UNION ALL

    -- Data fra SerpApi
    SELECT
      job_id,
      title,
      company_name,
      location,
      source AS scraped_from,
      scrapedAt AS scraped_at,
      description,
      'google.com' AS job_listing_domain,

      -- Udtræk alle unikke domæner fra links
      ARRAY(
      SELECT DISTINCT REGEXP_EXTRACT(link, r"https?://([^/]+)")
      FROM UNNEST(apply_options)
      WHERE link IS NOT NULL
    ) AS job_details_domain,

      share_link AS job_summary_url,

      -- Brug første ansøgningslink som job_description_url
      (
        SELECT link FROM UNNEST(apply_options) WHERE link IS NOT NULL LIMIT 1
      ) AS job_description_url

    FROM jobdata.raw_serpapi
  )
),

-- Behold kun nyeste version per job_id
latest AS (
  SELECT *, FROM (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY job_id ORDER BY scraped_at DESC) AS rn
  FROM merged
  )
  WHERE rn = 1
),

-- rolle baseret på titel eller beskrivelse (prioriterer title over description)
matched_roles_title AS (
  SELECT
    latest.job_id,
    rd.role_id,
    rd.role_name
  FROM latest
  JOIN jobdata.roles rd
    ON REGEXP_CONTAINS(LOWER(latest.title), rd.pattern)
  QUALIFY ROW_NUMBER() OVER (
    PARTITION BY latest.job_id
    ORDER BY rd.role_id
  ) = 1
),

-- Fallback hvis der ikke er rolle fundet i title
matched_roles_description AS (
  SELECT
    latest.job_id,
    rd.role_id,
    rd.role_name
  FROM latest
  JOIN jobdata.roles rd
    ON REGEXP_CONTAINS(LOWER(latest.description), rd.pattern)
  WHERE latest.job_id NOT IN (SELECT job_id FROM matched_roles_title)
  QUALIFY ROW_NUMBER() OVER (
    PARTITION BY latest.job_id
    ORDER BY rd.role_id
  ) = 1
),

-- Kombinér begge rolle-CTEs
matched_roles AS (
  SELECT * FROM matched_roles_title
  UNION ALL
  SELECT * FROM matched_roles_description
),

-- senioritet/erfaringsniveau baseret på titel eller beskrivelse (prioriterer title over description)
matched_levels_title AS (
  SELECT
    latest.job_id,
    ld.level_id,
    ld.level_name
  FROM latest
  JOIN jobdata.levels ld
    ON REGEXP_CONTAINS(LOWER(latest.title), ld.pattern)
  QUALIFY ROW_NUMBER() OVER (
    PARTITION BY latest.job_id
    ORDER BY ld.level_id
  ) = 1
),

-- Fallback hvis der ikke er niveau fundet i title
matched_levels_description AS (
  SELECT
    latest.job_id,
    ld.level_id,
    ld.level_name
  FROM latest
  JOIN jobdata.levels ld
    ON REGEXP_CONTAINS(LOWER(latest.description), ld.pattern)
  WHERE latest.job_id NOT IN (SELECT job_id FROM matched_levels_title)
  QUALIFY ROW_NUMBER() OVER (
    PARTITION BY latest.job_id
    ORDER BY ld.level_id
  ) = 1
),

-- Kombiner begge senioritets-CTEs
matched_levels AS (
  SELECT * FROM matched_levels_title
  UNION ALL
  SELECT * FROM matched_levels_description
),

-- programmeringssprog
matched_languages AS (
  SELECT
    latest.job_id,
    ARRAY_AGG(DISTINCT programming_languages.language_name) AS programming_languages
  FROM latest
  JOIN jobdata.programming_languages
    ON REGEXP_CONTAINS(latest.description, programming_languages.pattern)
  GROUP BY latest.job_id
),

-- web frameworks og teknologier
matched_frameworks AS (
  SELECT
    latest.job_id,
    ARRAY_AGG(DISTINCT fw.framework_name) AS frameworks
  FROM latest
  JOIN jobdata.web_frameworks_and_technologies AS fw
    ON REGEXP_CONTAINS(latest.description, fw.pattern)
  GROUP BY latest.job_id
),

-- databaser
matched_databases AS (
  SELECT
    latest.job_id,
    ARRAY_AGG(DISTINCT d.db_name) AS databases
  FROM latest
  JOIN jobdata.databases_and_storage d
    ON REGEXP_CONTAINS(latest.description, d.pattern)
  GROUP BY latest.job_id
),

-- øvrige tekniske kompetencer/skills
matched_skills AS (
  SELECT
    latest.job_id,
    ARRAY_AGG(DISTINCT skills.skill_name) AS skills
  FROM latest
  JOIN jobdata.skills
    ON REGEXP_CONTAINS(latest.description, skills.pattern)
  GROUP BY latest.job_id
)

-- Endelig SELECT: saml alt sammen i det færdige datasæt
SELECT
  latest.*,
    COALESCE(matched_roles.role_id, 1) AS role_id,
    rd.role_name,
    COALESCE(matched_levels.level_id, 1) AS level_id,
    ld.level_name,

  -- Marker om jobbet kan være fjernarbejde / remote
  CASE
    WHEN title LIKE '%remote%'
      OR location LIKE '%remote%'
      OR location LIKE '%mulighed for hjemmearbejde%'
      OR location LIKE '%work from home%'
      OR description LIKE '%remote%'
      OR description LIKE '%mulighed for hjemmearbejde%'
      OR description LIKE '%work from home%'
    THEN TRUE
    ELSE FALSE
  END AS remote_friendly,

  IFNULL(langs.programming_languages, ['unknown']) AS programming_languages,
  IFNULL(fws.frameworks, ['unknown']) AS frameworks,
  IFNULL(dbs.databases, ['unknown']) AS databases,
  IFNULL(sks.skills, ['unknown']) AS skills,

FROM latest
LEFT JOIN matched_roles ON latest.job_id = matched_roles.job_id
LEFT JOIN matched_levels ON latest.job_id = matched_levels.job_id

LEFT JOIN jobdata.roles rd ON COALESCE(matched_roles.role_id, 1) = rd.role_id
LEFT JOIN jobdata.levels ld ON COALESCE(matched_levels.level_id, 1) = ld.level_id

LEFT JOIN matched_languages langs ON latest.job_id = langs.job_id
LEFT JOIN matched_frameworks fws ON latest.job_id = fws.job_id
LEFT JOIN matched_databases dbs ON latest.job_id = dbs.job_id
LEFT JOIN matched_skills sks ON latest.job_id = sks.job_id;