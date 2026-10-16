# ATLAS — Backend API (`src/backend`)

## [A] Architect
- **Purpose:** local service holding accounts, sessions, documents, transcripts, settings; hosts the STT and Copilot pipelines; serves REST + WebSockets.
- **Users:** desktop renderer, desktop main process, browser extension (paired).
- **Constraints:** binds `127.0.0.1:5480` only; SQLite in the OS user-data folder; no keys persisted; Mode C must be able to host it unchanged apart from Infrastructure config.
- **Tech:** .NET 10 minimal APIs, EF Core 10 + SQLite, ASP.NET Core `PasswordHasher`, QuestPDF, ServiceDefaults (OpenTelemetry, health).
- **Out of scope:** hosting, Postgres provider wiring (Mode C), external identity.

## [T] Trace — create session
1. `POST /api/sessions` with mode, length, framework, pinned type, prompt → validate → persist → 201.
2. `POST /api/sessions/{id}/documents` (multipart) → reject if 10 already attached → extract text → chunk → persist.
3. `GET /api/sessions/{id}` → session + documents metadata.

## [L] Link
| From | To | Method | Contract | Failure mode |
|------|----|--------|----------|--------------|
| ui | Api | REST | OpenAPI at `/openapi/v1.json` | 4xx with ProblemDetails |
| extension | Api | REST/WS | `X-Pairing-Token` header | 401 |
| Api | SQLite | EF Core | migrations | DB locked → retry |

## [A] Assemble
1. Domain + EF model + migration. 2. Sessions/Documents endpoints. 3. Auth (local). 4. Transcripts + PDF. 5. Settings. 6. Pairing middleware.

## [S] Stress-test
- 10 documents × 50 pages each ingest without blocking the request thread.
- Malformed upload (wrong type, >20 MB) → 400 ProblemDetails, nothing stored.
- Request from non-loopback address → refused (binding).
- Password hash verification time stays < 250 ms.
