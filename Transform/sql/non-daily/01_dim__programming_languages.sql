CREATE OR REPLACE TABLE jobdata.programming_languages AS
SELECT * FROM UNNEST([

  -- Compiled languages
  -- Compiled languages
  STRUCT('c#'           AS language_name, r'(?i)(^|[^a-z0-9])c#([^a-z0-9]|$)'                       AS pattern),
  STRUCT('c++'          AS language_name, r'(?i)(^|[^a-z0-9])c\+\+([^a-z0-9]|$)'                    AS pattern),
  STRUCT('f#'           AS language_name, r'(?i)(^|[^a-z0-9])f#([^a-z0-9]|$)'                       AS pattern),
  STRUCT('java'         AS language_name, r'(?i)\bjava\b'                                           AS pattern),
  STRUCT('go'           AS language_name, r'(?i)\bgo(lang)?\b'                                      AS pattern),
  STRUCT('swift'        AS language_name, r'(?i)\bswift\b'                                          AS pattern),
  STRUCT('dart'         AS language_name, r'(?i)\bdart\b'                                           AS pattern),
  STRUCT('rust'         AS language_name, r'(?i)\brust\b'                                           AS pattern),
  STRUCT('scala'        AS language_name, r'(?i)\bscala\b'                                          AS pattern),
  STRUCT('objective-c'  AS language_name, r'(?i)(^|[^a-z0-9])objective[ -]?c([^a-z0-9]|$)'          AS pattern),
  STRUCT('assembly'     AS language_name, r'(?i)\bassembly\b'                                       AS pattern),
  STRUCT('kotlin'       AS language_name, r'(?i)\bkotlin\b'                                         AS pattern),

  -- Scripting languages
  STRUCT('python'       AS language_name, r'(?i)\bpython\b'                                         AS pattern),
  STRUCT('javascript'   AS language_name, r'(?i)(^|[^a-z0-9])(js|javascript)([^a-z0-9]|$)'          AS pattern),
  STRUCT('typescript'   AS language_name, r'(?i)(^|[^a-z0-9])(ts|typescript)([^a-z0-9]|$)'          AS pattern),
  STRUCT('php'          AS language_name, r'(?i)\bphp\b'                                            AS pattern),
  STRUCT('ruby'         AS language_name, r'(?i)\bruby\b'                                           AS pattern),
  STRUCT('lua'          AS language_name, r'(?i)\blua\b'                                            AS pattern),

  -- Shell & automation
  STRUCT('bash'         AS language_name, r'(?i)(^|[^a-z0-9])(bash|shell script)([^a-z0-9]|$)'      AS pattern),
  STRUCT('shell'        AS language_name, r'(?i)(^|[^a-z0-9])(shell scripting|shell)([^a-z0-9]|$)'  AS pattern),
  STRUCT('powershell'   AS language_name, r'(?i)\bpowershell\b'                                     AS pattern),

  -- Markup & styling
  STRUCT('html/css'     AS language_name, r'(?i)\b(html|css|html\/css)\b'                           AS pattern),

  -- Query/data languages
  STRUCT('sql'          AS language_name, r'(?i)\bsql\b'                                            AS pattern)
]);
