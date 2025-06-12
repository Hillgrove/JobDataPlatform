CREATE OR REPLACE TABLE jobdata.roles (
  role_id INT64,
  role_name STRING,
  pattern STRING
);

INSERT INTO jobdata.roles (role_id, role_name, pattern)
VALUES
  (1, "unknown", r"xxxxxTHISSHOULDNEVERMATCHxxxxx"),
  (2, "frontend", r"(?i)\b(front[\s\-]?end|ui|ux)\b"),
  (3, "backend", r"(?i)\bback[\s\-]?end\b"),
  (4, "fullstack", r"(?i)\bfull[\s\-]?stack\b"),
  (5, "devops", r"(?i)\bdev[\s\-]?ops\b"),
  (6, "data engineer", r"(?i)\bdata[\s\-]?engineer(s)?\b"),
  (7, "ml / data science / ai", r"(?i)\b(machine[\s\-]?learning|ml|ai|data[\s\-]?science)\b"),
  (8, "qa / test", r"(?i)\b(qa|quality[\s\-]?assurance|test(er|ing)?)\b"),
  (9, "embedded", r"(?i)\b(embedded|firmware|plc)\b"),
  (10, "architect", r"(?i)\b(software[\s\-]?)?architect(s)?\b"),
  (11, "business intelligence", r"(?i)\b(business[\s\-]?intelligence|bi)\b"),
  (12, "sre", r"(?i)\bsre\b");
