# JobData Platform

[![ELT Pipeline](https://github.com/Hillgrove/JobDataPlatform/actions/workflows/ELT-pipeline.yml/badge.svg)](https://github.com/Hillgrove/JobDataPlatform/actions/workflows/ELT-pipeline.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Google Cloud](https://img.shields.io/badge/Google_Cloud-4285F4?logo=google-cloud&logoColor=white)](https://cloud.google.com/)
[![BigQuery](https://img.shields.io/badge/BigQuery-Datavarehus-669DF6?logo=google-bigquery&logoColor=white)](https://cloud.google.com/bigquery)

> En fuldt automatiseret ELT-pipeline der dagligt indsamler danske jobopslag fra Jobindex og Google Jobs, loader dem i Google Cloud og transformerer dem til et analyseklart datavarehus i BigQuery.

---

## Indhold

- [Om projektet](#om-projektet)
- [Arkitektur](#arkitektur)
- [Tech stack](#tech-stack)
- [Datamodel](#datamodel)
- [Kom i gang](#kom-i-gang)
- [CI/CD](#cicd)

---

## Om projektet

JobData Platform indsamler løbende jobopslag fra det danske arbejdsmarked og bearbejder dem til strukturerede, analyseklare data. Systemet henter data fra to kilder:

- **Jobindex.dk** — via RSS-feed med efterfølgende HTML-scraping af stillingsbeskrivelser
- **Google Jobs** — via SerpApi, der aggregerer jobopslag på tværs af platforme

Rådataene berige med rolletyper, senioritetsniveauer, teknologier og kompetencer gennem mønsterbaseret matching. Resultatet er et flerlags BigQuery-datavarehus, der muliggør analyse af det danske IT-jobmarked.

---

## Arkitektur

Systemet er bygget op som en klassisk **ELT-pipeline** (Extract → Load → Transform) fordelt over fire C#-moduler:

```mermaid
flowchart LR
    classDef source  fill:#F4A261,stroke:#E76F51,color:#fff
    classDef stage   fill:#E9C46A,stroke:#d4a017,color:#333
    classDef storage fill:#4895EF,stroke:#4361EE,color:#fff
    classDef raw     fill:#90E0EF,stroke:#4895EF,color:#333
    classDef enrich  fill:#52B788,stroke:#2D6A4F,color:#fff
    classDef dimrel  fill:#9B5DE5,stroke:#7B2FBE,color:#fff
    classDef fact    fill:#3A0CA3,stroke:#240046,color:#fff

    subgraph SRC["Datakilder"]
        A1["Jobindex.dk"]:::source
        A2["Google Jobs"]:::source
    end

    B["NDJSON filer"]:::stage

    subgraph GCP["Google Cloud"]
        C["Cloud Storage"]:::storage
        subgraph BQ["BigQuery"]
            D["Raw"]:::raw
            E["Enriched"]:::enrich
            F["Dim + Rel"]:::dimrel
            G["Fact + Views"]:::fact
        end
    end

    A1 & A2 --> B --> C --> D --> E --> F --> G
```

BigQuery-tabellernes indbyrdes afhængigheder:

```mermaid
flowchart TD
    classDef raw    fill:#90E0EF,stroke:#4895EF,color:#333
    classDef ref    fill:#F4A261,stroke:#E76F51,color:#fff
    classDef enrich fill:#52B788,stroke:#2D6A4F,color:#fff
    classDef dim    fill:#4895EF,stroke:#4361EE,color:#fff
    classDef rel    fill:#9B5DE5,stroke:#7B2FBE,color:#fff
    classDef fact   fill:#3A0CA3,stroke:#240046,color:#fff
    classDef rep    fill:#560BAD,stroke:#3A0CA3,color:#fff

    R1["raw_jobindex"]:::raw
    R2["raw_serpapi"]:::raw
    REF["ref_* tabeller"]:::ref
    E["enriched_job_listings"]:::enrich
    D1["dim_companies"]:::dim
    D2["dim_domains"]:::dim
    D3["dim_technologies"]:::dim
    RL1["rel_job_details_domains"]:::rel
    RL2["rel_job_technologies"]:::rel
    F["fct_jobs"]:::fact
    V1["rep_jobs_exploded"]:::rep
    V2["rep_jobs_flattened"]:::rep

    R1 & R2 & REF --> E
    E --> D1 & D2 & D3 & RL1 & RL2
    D1 & D2 & D3 & RL1 & RL2 --> F
    F --> V1 & V2
```

**Moduloversigt:**

| Modul | Ansvar |
|---|---|
| `CLI` | Orkestrerer hele pipelinen — kalder Extract → Upload → Load → Transform |
| `Extract` | Scraper Jobindex og kalder SerpApi; outputter NDJSON-filer |
| `DataTransfer` | Uploader filer til GCS; loader GCS-data ind i BigQuery råtabeller |
| `Transform` | Eksekverer SQL-transformationer i BigQuery (dagligt og fuld genopbygning) |

---

## Tech stack

| Kategori | Teknologi | Anvendelse |
|---|---|---|
| **Sprog** | C# / .NET 8 | Al applikationskode |
| **Datavarehus** | Google BigQuery | Opbevaring og transformation af data |
| **Cloud storage** | Google Cloud Storage | Staging af rå NDJSON-filer |
| **API** | SerpApi | Adgang til Google Jobs-opslag |
| **HTML-parsing** | HtmlAgilityPack | Scraping af stillingsbeskrivelser fra Jobindex |
| **RSS-parsing** | System.ServiceModel.Syndication | Læsning af Jobindex RSS-feed |
| **Serialisering** | Newtonsoft.Json | NDJSON-output og API-respons-parsing |
| **CI/CD** | GitHub Actions | Automatiseret kørsel af pipelinen |

---

## Datamodel

BigQuery-datasættet er opbygget i lag, der svarer til et klassisk dimensionelt datawarehouse:

```
Lag 0 — Rå data
  raw_jobindex              Ubehandlede jobopslag fra Jobindex (partitioneret pr. dag)
  raw_serpapi               Ubehandlede jobopslag fra Google Jobs (partitioneret pr. dag)

Lag 1 — Referencetabeller
  ref_roles                 Jobrolle-kategorier (fx Backend Developer, Data Engineer)
  ref_levels                Senioritetsniveauer (fx Junior, Senior, Lead)
  ref_skills                Kompetencer og teknologier
  ref_programming_languages Programmeringssprog
  ref_databases_and_storage Databaseteknologier
  ref_web_frameworks_and_technologies  Frameworks og webteknologier

Lag 2 — Enriched
  enriched_job_listings     Normaliserede og deduplikerede jobopslag fra begge kilder,
                            beriget med roller, niveauer og kompetencer via MERGE

Lag 3 — Dimensioner
  dim_companies             Virksomhedsdimension
  dim_domains               Fagdomæne-dimension
  dim_technologies          Teknologidimension
  dim_stop_words            Stopord til tekstbehandling

Lag 4 — Relationer (N:M)
  rel_job_details_domains   Kobling mellem jobopslag og fagdomæner
  rel_job_technologies      Kobling mellem jobopslag og teknologier

Lag 5 — Fakta og rapportering
  fct_jobs                  Faktabel til analytiske forespørgsler
  rep_jobs_exploded         Rapporteringsview med eksploderede arrays
  rep_jobs_flattened        Rapporteringsview med fladede strukturer
```

Transformationerne kører i to modi:
- **Daily** (`Transform/sql/daily/`) — inkrementel opdatering via `MERGE` på nye data
- **Non-daily** (`Transform/sql/non-daily/`) — fuld genopbygning af alle tabeller

---

## Kom i gang

### Forudsætninger

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- En Google Cloud-konto med BigQuery og Cloud Storage aktiveret
- En GCP Service Account med nøgle (JSON)
- En [SerpApi](https://serpapi.com/)-nøgle

### Opsætning

**1. Klon repositoriet**

```bash
git clone https://github.com/Hillgrove/JobDataPlatform.git
cd JobDataPlatform
```

**2. Placer GCP-nøglen**

```bash
# Opret mappen og placer din service account-nøgle her:
DataTransfer/Secrets/gcs-key.json
```

> Filen er listet i `.gitignore` og bliver aldrig committed.

**3. Sæt miljøvariabel**

```bash
export SERP_API_KEY="din-serp-api-nøgle"
```

**4. Kør pipelinen**

```bash
dotnet restore CLI/CLI.csproj
dotnet run --project CLI/CLI.csproj
```

Pipelinen kører hele flowet: Extract → Upload → Load → Transform.

---

## CI/CD

Pipelinen eksekveres automatisk via **GitHub Actions** (`.github/workflows/ELT-pipeline.yml`).

Workflowet kan trigges manuelt via `workflow_dispatch` i GitHub-brugerfladen og konfigureres til at køre nightly via cron.

**Secrets der skal konfigureres i GitHub:**

| Secret | Beskrivelse |
|---|---|
| `GCP_SA_KEY` | Base64-encoded GCP service account JSON |
| `SERP_API_KEY` | API-nøgle til SerpApi |

Workflowet decoder automatisk `GCP_SA_KEY` og gemmer nøglen til `DataTransfer/Secrets/gcs-key.json` inden pipelinen startes.
