CREATE OR REPLACE TABLE jobdata.skills AS
SELECT * FROM UNNEST([
  STRUCT('docker'      AS skill_name, r'(?i)\bdocker\b'        AS pattern),
  STRUCT('kubernetes'  AS skill_name, r'(?i)\bkubernetes\b'    AS pattern),
  STRUCT('azure'       AS skill_name, r'(?i)\bazure\b'         AS pattern),
  STRUCT('aws'         AS skill_name, r'(?i)\baws\b'           AS pattern),
  STRUCT('git'         AS skill_name, r'(?i)\bgit\b'           AS pattern),
  STRUCT('graphql'     AS skill_name, r'(?i)\bgraphql\b'       AS pattern),
  STRUCT('ci/cd'       AS skill_name, r'(?i)\bci/cd\b'         AS pattern)
]);
