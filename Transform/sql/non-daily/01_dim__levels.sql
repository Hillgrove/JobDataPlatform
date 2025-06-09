CREATE OR REPLACE TABLE jobdata.levels (
  level_id INT64,
  level_name STRING,
  pattern STRING
);

INSERT INTO jobdata.levels (level_id, level_name, pattern)
VALUES
  (1, "unknown",    r"xxxxxTHISSHOULDNEVERMATCHxxxxx"),
  (2, "intern",     r"(?i)\b(internship|praktikant|praktik)\b"),
  (3, "student",    r"(?i)\bstudent(er|medhjælp)?\b"),
  (4, "junior",     r"(?i)\bjunior\b"),
  (5, "medior",     r"(?i)\bmedior\b"),
  (6, "senior",     r"(?i)\bsenior\b"),
  (7, "lead",       r"(?i)\b(tech lead|team lead|lead developer|lead engineer|lead)\b"),
  (8, "principal",  r"(?i)\bprincipal\b"),
  (9, "manager",    r"(?i)\b(manager|project manager)\b"),
  (10, "head",      r"(?i)\b(head of|head)\b"),
  (11, "director",  r"(?i)\bdirector\b"),
  (12, "cto",       r"(?i)\b(chief technology officer|cto)\b");
