CREATE OR REPLACE TABLE jobdata.databases_and_storage AS
SELECT * FROM UNNEST([

  STRUCT('postgresql'                   AS db_name, r'(?i)\b(postgre(s|sql)|pg)\b'                      AS pattern),
  STRUCT('mysql'                        AS db_name, r'(?i)\bmy\s?sql\b'                                 AS pattern),
  STRUCT('sqlite'                       AS db_name, r'(?i)\bsqlite\b'                                   AS pattern),
  STRUCT('microsoft sql server'         AS db_name, r'(?i)\b(sql\s?server|microsoft\s?sql)\b'           AS pattern),
  STRUCT('mongodb'                      AS db_name, r'(?i)\bmongo\s?db\b'                               AS pattern),
  STRUCT('redis'                        AS db_name, r'(?i)\bredis\b'                                    AS pattern),
  STRUCT('mariadb'                      AS db_name, r'(?i)\bmaria\s?db\b'                               AS pattern),
  STRUCT('elasticsearch'                AS db_name, r'(?i)\belastic\s?search\b'                         AS pattern),
  STRUCT('oracle'                       AS db_name, r'(?i)\boracle\b'                                   AS pattern),
  STRUCT('dynamodb'                     AS db_name, r'(?i)\bdynamo\s?db\b'                              AS pattern),
  STRUCT('firebase realtime database'   AS db_name, r'(?i)\bfirebase\srealtime\b'                       AS pattern),
  STRUCT('cloud firestore'              AS db_name, r'(?i)\bcloud\sfirestore\b'                         AS pattern),
  STRUCT('bigquery'                     AS db_name, r'(?i)\bbig\s?query\b'                              AS pattern),
  STRUCT('microsoft access'             AS db_name, r'(?i)\b(access|microsoft\saccess)\b'               AS pattern),
  STRUCT('supabase'                     AS db_name, r'(?i)\bsupabase\b'                                 AS pattern)

]);
