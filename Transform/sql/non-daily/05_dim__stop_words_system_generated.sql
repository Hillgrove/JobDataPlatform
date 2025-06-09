-- Genererer stopord baseret på eksisterende matcher-tabeller:
-- role_definitions, level_definitions, language_aliases, framework_aliases, skill_aliases.

CREATE OR REPLACE VIEW jobdata.stop_words_system_generated AS
SELECT LOWER(role_name) AS word FROM jobdata.roles
UNION DISTINCT
SELECT LOWER(level_name) FROM jobdata.levels
UNION DISTINCT
SELECT LOWER(language_name) FROM jobdata.programming_languages
UNION DISTINCT
SELECT LOWER(framework_name) FROM jobdata.frameworks
UNION DISTINCT
SELECT LOWER(skill_name) FROM jobdata.skills;
