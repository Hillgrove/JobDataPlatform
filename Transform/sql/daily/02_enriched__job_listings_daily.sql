MERGE INTO jobdata.enriched_job_listings as target
USING (
  WITH merged AS (
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
      ARRAY(
        SELECT LOWER(domain)
        FROM UNNEST(job_details_domain) AS domain
      ) AS job_details_domain,
      LOWER(job_summary_url) AS job_summary_url,
      LOWER(job_description_url) AS job_description_url
    FROM (
      SELECT
        id AS job_id,
        REGEXP_REPLACE(titel, r',\s*[^,]+$', '') AS title,
        TRIM(REGEXP_EXTRACT(titel, r',\s*([^,]+)$')) AS company_name,
        REGEXP_EXTRACT(shortDescriptionHtml, r'<span[^>]*class="jix_robotjob--area"[^>]*>([^<]+)</span>') AS location,
        source AS scraped_from,
        scrapedAt AS scraped_at,
        TRIM(REGEXP_REPLACE(
          REGEXP_REPLACE(COALESCE(fullDescriptionHtml, shortDescriptionHtml), r'<[^>]+>', ' '),
          r'\s+', ' '
        )) AS description,
        'jobindex.dk' AS job_listing_domain,
        [REGEXP_EXTRACT(seeJobUrl, r"https?://([^/]+)")] AS job_details_domain,
        summaryUrl AS job_summary_url,
        seeJobUrl AS job_description_url
      FROM jobdata.raw_jobindex

      UNION ALL

      SELECT
        job_id,
        title,
        company_name,
        location,
        source AS scraped_from,
        scrapedAt AS scraped_at,
        description,
        'google.com' AS job_listing_domain,
        ARRAY(
          SELECT DISTINCT REGEXP_EXTRACT(link, r"https?://([^/]+)")
          FROM UNNEST(apply_options)
          WHERE link IS NOT NULL
        ) AS job_details_domain,
        share_link AS job_summary_url,
        (
          SELECT link FROM UNNEST(apply_options) WHERE link IS NOT NULL LIMIT 1
        ) AS job_description_url
      FROM jobdata.raw_serpapi
    )
  ),

  latest AS (
    SELECT *
    FROM (
      SELECT *, ROW_NUMBER() OVER (PARTITION BY job_id ORDER BY scraped_at DESC) AS rn
      FROM merged
    )
    WHERE rn = 1 AND scraped_at = CURRENT_DATE()
  ),

  matched_roles_title AS (
    SELECT latest.job_id, rd.role_id, rd.role_name
    FROM latest
    JOIN jobdata.roles rd ON REGEXP_CONTAINS(LOWER(latest.title), rd.pattern)
    QUALIFY ROW_NUMBER() OVER (PARTITION BY latest.job_id ORDER BY rd.role_id) = 1
  ),

  matched_roles_description AS (
    SELECT latest.job_id, rd.role_id, rd.role_name
    FROM latest
    JOIN jobdata.roles rd ON REGEXP_CONTAINS(LOWER(latest.description), rd.pattern)
    WHERE latest.job_id NOT IN (SELECT job_id FROM matched_roles_title)
    QUALIFY ROW_NUMBER() OVER (PARTITION BY latest.job_id ORDER BY rd.role_id) = 1
  ),

  matched_roles AS (
    SELECT * FROM matched_roles_title
    UNION ALL
    SELECT * FROM matched_roles_description
  ),

  matched_levels_title AS (
    SELECT latest.job_id, ld.level_id, ld.level_name
    FROM latest
    JOIN jobdata.levels ld ON REGEXP_CONTAINS(LOWER(latest.title), ld.pattern)
    QUALIFY ROW_NUMBER() OVER (PARTITION BY latest.job_id ORDER BY ld.level_id) = 1
  ),

  matched_levels_description AS (
    SELECT latest.job_id, ld.level_id, ld.level_name
    FROM latest
    JOIN jobdata.levels ld ON REGEXP_CONTAINS(LOWER(latest.description), ld.pattern)
    WHERE latest.job_id NOT IN (SELECT job_id FROM matched_levels_title)
    QUALIFY ROW_NUMBER() OVER (PARTITION BY latest.job_id ORDER BY ld.level_id) = 1
  ),

  matched_levels AS (
    SELECT * FROM matched_levels_title
    UNION ALL
    SELECT * FROM matched_levels_description
  ),

  matched_languages AS (
    SELECT latest.job_id, ARRAY_AGG(DISTINCT programming_languages.language_name) AS programming_languages
    FROM latest
    JOIN jobdata.programming_languages ON REGEXP_CONTAINS(latest.description, programming_languages.pattern)
    GROUP BY latest.job_id
  ),

  matched_frameworks AS (
    SELECT latest.job_id, ARRAY_AGG(DISTINCT frameworks.framework_name) AS frameworks
    FROM latest
    JOIN jobdata.frameworks ON REGEXP_CONTAINS(latest.description, frameworks.pattern)
    GROUP BY latest.job_id
  ),

  matched_databases AS (
    SELECT latest.job_id,ARRAY_AGG(DISTINCT d.db_name) AS databases
    FROM latest
    JOIN jobdata.databases_and_storage d ON REGEXP_CONTAINS(latest.description, d.pattern)
    GROUP BY latest.job_id
  ),

  matched_skills AS (
    SELECT latest.job_id, ARRAY_AGG(DISTINCT skills.skill_name) AS skills
    FROM latest
    JOIN jobdata.skills ON REGEXP_CONTAINS(latest.description, skills.pattern)
    GROUP BY latest.job_id
  )

  SELECT
    latest.*,
    COALESCE(matched_roles.role_id, 1) AS role_id,
    rd.role_name,
    COALESCE(matched_levels.level_id, 1) AS level_id,
    ld.level_name,

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

    IFNULL(matched_languages.programming_languages, ['unknown']) AS programming_languages,
    IFNULL(matched_frameworks.frameworks, ['unknown']) AS frameworks,
    IFNULL(dbs.databases, ['unknown']) AS databases,
    IFNULL(matched_skills.skills, ['unknown']) AS skills

  FROM latest
  LEFT JOIN matched_roles ON latest.job_id = matched_roles.job_id
  LEFT JOIN matched_levels ON latest.job_id = matched_levels.job_id
  LEFT JOIN jobdata.roles rd ON COALESCE(matched_roles.role_id, 1) = rd.role_id
  LEFT JOIN jobdata.levels ld ON COALESCE(matched_levels.level_id, 1) = ld.level_id
  LEFT JOIN matched_languages ON latest.job_id = matched_languages.job_id
  LEFT JOIN matched_frameworks ON latest.job_id = matched_frameworks.job_id
  LEFT JOIN matched_databases dbs ON latest.job_id = dbs.job_id
  LEFT JOIN matched_skills ON latest.job_id = matched_skills.job_id

) AS source

ON target.job_id = source.job_id

WHEN MATCHED AND target.scraped_at < source.scraped_at THEN
  UPDATE SET
    title = source.title,
    company_name = source.company_name,
    location = source.location,
    scraped_from = source.scraped_from,
    scraped_at = source.scraped_at,
    description = source.description,
    job_listing_domain = source.job_listing_domain,
    job_details_domain = source.job_details_domain,
    job_summary_url = source.job_summary_url,
    job_description_url = source.job_description_url,
    role_id = source.role_id,
    role_name = source.role_name,
    level_id = source.level_id,
    level_name = source.level_name,
    remote_friendly = source.remote_friendly,
    programming_languages = source.programming_languages,
    frameworks = source.frameworks,
    databases = source.databases,
    skills = source.skills

WHEN NOT MATCHED THEN
  INSERT (
    job_id, title, company_name, location, scraped_from, scraped_at,
    description, job_listing_domain, job_details_domain,
    job_summary_url, job_description_url,
    role_id, role_name, level_id, level_name, remote_friendly,
    programming_languages, frameworks, databases, skills
  )
  VALUES (
    source.job_id, source.title, source.company_name, source.location, source.scraped_from, source.scraped_at,
    source.description, source.job_listing_domain, source.job_details_domain,
    source.job_summary_url, source.job_description_url,
    source.role_id, source.role_name, source.level_id, source.level_name, source.remote_friendly,
    source.programming_languages, source.frameworks, source.databases, source.skills
  );
