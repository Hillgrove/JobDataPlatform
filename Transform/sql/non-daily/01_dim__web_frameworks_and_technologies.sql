CREATE OR REPLACE TABLE jobdata.web_frameworks_and_technologies AS
SELECT * FROM UNNEST([
  -- Moderne frontend frameworks
  STRUCT('react'         AS framework_name, r'(?i)\breact(\.js)?\b'             AS pattern),
  STRUCT('angular'       AS framework_name, r'(?i)\bangular(?!js)\b'            AS pattern), -- ekskluder "angularjs"
  STRUCT('angularjs'     AS framework_name, r'(?i)\bangular\s?js\b'             AS pattern),
  STRUCT('vue'           AS framework_name, r'(?i)\bvue(\.js)?\b'               AS pattern),
  STRUCT('svelte'        AS framework_name, r'(?i)\bsvelte\b'                   AS pattern),
  STRUCT('sveltekit'     AS framework_name, r'(?i)\bsvelte\s?kit\b'             AS pattern),
  STRUCT('next.js'       AS framework_name, r'(?i)\bnext(\.js)?\b'              AS pattern),
  STRUCT('nuxt.js'       AS framework_name, r'(?i)\bnuxt(\.js)?\b'              AS pattern),
  STRUCT('htmx'          AS framework_name, r'(?i)\bhtmx\b'                     AS pattern),

  -- Backend frameworks og sprogrelaterede
  STRUCT('express'       AS framework_name, r'(?i)\bexpress(\.js)?\b'           AS pattern),
  STRUCT('nest.js'       AS framework_name, r'(?i)\bnest(\.js)?\b'              AS pattern),
  STRUCT('spring'        AS framework_name, r'(?i)\bspring\s?(boot|framework)?' AS pattern),
  STRUCT('.net'          AS framework_name, r'(?i)\.net(?!\s?(core))'           AS pattern),
  STRUCT('asp.net core'  AS framework_name, r'(?i)\basp\.net\s?core\b'          AS pattern),
  STRUCT('asp.net'       AS framework_name, r'(?i)\basp\.net\b'                 AS pattern),
  STRUCT('blazor'        AS framework_name, r'(?i)\bblazor\b'                   AS pattern),
  STRUCT('django'        AS framework_name, r'(?i)\bdjango\b'                   AS pattern),
  STRUCT('flask'         AS framework_name, r'(?i)\bflask\b'                    AS pattern),
  STRUCT('fastapi'       AS framework_name, r'(?i)\bfastapi\b'                  AS pattern),
  STRUCT('laravel'       AS framework_name, r'(?i)\blaravel\b'                  AS pattern),
  STRUCT('ruby on rails' AS framework_name, r'(?i)\bruby\s?on\s?rails\b'        AS pattern),
  STRUCT('wordpress'     AS framework_name, r'(?i)\bwordpress\b'                AS pattern),

  -- Øvrige udbredte
  STRUCT('jquery'        AS framework_name, r'(?i)\bjquery\b'                   AS pattern),
  STRUCT('node.js'       AS framework_name, r'(?i)\bnode\.?js\b'                AS pattern)
]);
