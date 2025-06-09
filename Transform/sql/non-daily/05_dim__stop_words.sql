-- Kombinerer system-genererede og manuelt definerede stopord i én samlet view.
-- Brug dette view i explore-unmatched queries til at filtrere alt der allerede er kendt eller uinteressant.

CREATE OR REPLACE VIEW jobdata.stop_words AS
SELECT word FROM jobdata.stop_words_manual
UNION DISTINCT
SELECT word FROM jobdata.stop_words_system_generated;


CREATE OR REPLACE VIEW jobdata.stop_words AS
SELECT word FROM jobdata.stop_words_manual
UNION DISTINCT
SELECT word FROM jobdata.stop_words_system_generated;
