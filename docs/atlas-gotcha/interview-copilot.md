# ATLAS + GOTCHA brief — InterviewCopilot (system level)

Status: approved at scaffold time (2026-10-15). Keep this file updated when requirements change.
Component-level checklists live in `atlas/`; each task gets its own GOTCHA prompt that references
`GOTCHA-PROJECT.md` instead of repeating it.

---

## ATLAS

### [A] Architect

- **Purpose:** suggest answers in real time during live conversations (interviews, sales calls,
  any high-stakes talk), and let users rehearse with a scored mock interviewer.
- **Users:** individual candidates/sellers running the app on their own machine. Later (Mode C):
  anyone using a hosted web version.
- **Constraints:** zero cost by default; 2–5 s from end of question to first answer token on a
  16 GB / no-GPU machine with local models; data stays on the machine by default; Windows 10 22H2+/11
  and macOS 13+; MIT licence; ≥75% test coverage.
- **Tech decisions:** Electron + React/Vite/TS/shadcn/Tailwind; .NET 10 Clean Architecture backend
  running locally; SQLite via EF Core; Whisper.net (default STT); Deepgram (disabled); Ollama
  (default LLM); BYOK cloud LLMs; MV3 extension for Chrome/Edge/Firefox; pnpm; Aspire for dev;
  GitHub Actions; GitHub Issues.
- **Out of scope (v1):** Android/mobile; hosted backend, web frontend, cloud storage and share
  links (Mode C); Google/GitHub sign-in (Mode C); code-signed installers; automatic CI security
  review with Claude (manual only); OpenTelemetry export endpoint; Terraform/cloud infrastructure;
  stealth/screen-share-hiding features; payments of any kind.

### [T] Trace — live answer flow (Mode A/B)

1. Desktop app detects a meeting (known meeting process running + audio activity) → **prompts the
   user** "Meeting detected — start InterviewCopilot?" (auto-start only if the setting is on).
2. User confirms → desktop app calls `POST /api/sessions/{id}/start` and opens the overlay.
3. Desktop captures **system audio** (interviewer) and **mic** (user) as two labelled PCM streams,
   16 kHz mono, chunked.
4. Chunks stream to the backend over a WebSocket (`/ws/sessions/{id}/audio`).
5. Backend STT provider (Whisper by default) produces transcript segments tagged `Interviewer` or
   `Candidate`; segments are persisted and pushed to clients (`/ws/sessions/{id}/events`).
6. Question detector decides an interviewer turn is a question (end-of-turn silence + intent check).
7. Classifier assigns a question type; pinned type is used unless auto-detect disagrees with
   sufficient confidence (auto-detect overrides).
8. Prompt builder combines session prompt, answer length, framework, question type, relevant
   document chunks (≤10 docs) and recent transcript.
9. LLM provider (Ollama by default) streams tokens → backend pushes `answer.delta` events.
10. Desktop overlay and (Mode B) extension overlay render the stream. User can press
    **Regenerate / Shorter / Longer**, which re-runs step 8–9 with a modifier.
11. Session ends → transcript + answers saved; user can export to PDF.

Failure points: no audio permission (macOS Screen Recording) at 3; Ollama not running at 9;
model too slow at 9; extension can't reach backend at 10.

### [L] Link

| From | To | Method | Contract | Failure mode |
|------|----|--------|----------|--------------|
| Desktop renderer (ui) | Backend | HTTP JSON (REST) | `/api/*`, versioned by OpenAPI doc | Backend down → banner "Backend not running", retry with backoff |
| Desktop main | Backend | WebSocket binary | `/ws/sessions/{id}/audio`, PCM16 16 kHz frames + speaker byte | Drop → buffer ≤5 s, reconnect |
| ui / extension | Backend | WebSocket JSON | `/ws/sessions/{id}/events`: `transcript.segment`, `question.detected`, `answer.delta`, `answer.done`, `error` | Reconnect, resume from last event id |
| Extension | Backend | HTTP + WS on `127.0.0.1:5480` | pairing token issued by desktop app | Unpaired → show "Open the desktop app to pair" |
| Backend | Whisper.net | in-process | `ISpeechToTextProvider` | Model missing → download prompt |
| Backend | Deepgram | WebSocket (cloud) | `ISpeechToTextProvider`, **disabled by config** | Key invalid → fall back to Whisper + notify |
| Backend | Ollama | HTTP `127.0.0.1:11434` | `ILlmProvider` streaming | Not running → actionable error event |
| Backend | Cloud LLMs (BYOK) | HTTPS | `ILlmProvider` | 401/429 → error event, no retry storm |
| Desktop main | OS keychain | Electron `safeStorage` | `IKeyStore` (desktop) → keys passed per request, never persisted by backend | Keychain unavailable → refuse to store, explain |
| Backend | SQLite | EF Core | migrations | Locked DB → retry, WAL mode |

### [A] Assemble

1. **Foundation:** domain model, provider interfaces, SQLite schema + migrations, API contracts,
   event contract, CI + governance.
2. **Core:** Sessions, Documents, Transcription (Whisper), Copilot (detect → classify → answer with
   Ollama), Overlay (desktop).
3. **Supporting:** Auth (local), Transcripts (history + PDF), Providers (BYOK + keychain), Settings,
   Practice (mock interviewer + scoring).
4. **Integration/Reliability:** meeting detection + consent prompt, extension companion + pairing,
   Deepgram adapter (disabled), reconnect/backoff, latency budget tests.
5. **Deployment:** electron-builder beta/release pipelines, extension packaging, README install docs.

### [S] Stress-test

| Scenario | Expectation |
|----------|-------------|
| Load | 60-minute session, ~12k transcript words, 10 docs attached → memory < 6 GB total incl. models, no UI stutter |
| Latency | P95 end-of-question → first answer token ≤ 5 s with defaults on reference hardware |
| Ollama down | Clear error in overlay within 1 s, transcript keeps recording |
| Backend crash | Desktop restarts it; session resumes from persisted state |
| Mic/system audio denied | Explain the exact OS permission to grant; no silent failure |
| Security | Backend binds to `127.0.0.1` only; extension requires pairing token; CORS restricted to app + extension origins; keys never logged or stored by backend |
| Data integrity | Transcript segments written in order with timestamps; export reproduces stored data exactly |
| Consent | No capture starts without a user click unless auto-start is explicitly enabled |

---

## GOTCHA (scaffold level)

=== GOALS ===
- A cloned repo builds, tests (≥75% coverage) and runs the desktop app + local backend with zero paid services.
- Every module has a home in backend/ui/desktop/extension as per the module table in `CLAUDE.md`.
- Mode C can be added without moving UI code or changing Application interfaces.

=== ORCHESTRATION ===
Follow the Assemble phases above; one GitHub issue per task (`scripts/create-issues.sh`).

=== TOOLS ===
.NET 10 SDK, EF Core 10 + SQLite, Whisper.net, QuestPDF (Community licence), xUnit, NetArchTest,
coverlet; Node 24, pnpm 10, Electron, Vite, React 19, TypeScript, Tailwind 4, shadcn/ui, Vitest;
.NET Aspire; GitHub Actions; electron-builder; web-ext.

=== CONTEXT ===
See `GOTCHA-PROJECT.md`.

=== HEURISTICS ===
See `GOTCHA-PROJECT.md`.

=== ARGS ===
| Arg | Value |
|-----|-------|
| Backend URL | `http://127.0.0.1:5480` |
| App ID | `io.github.ukez15.interviewcopilot` |
| Repo | `ukez15/interview-copilot` |
| Max documents per session | 10 |
| Answer lengths | Short, Balanced, Long |
| Frameworks | None, STAR, SMART |
| Question types | Behavioral, HowDoYou, Situational, TechnicalKnowledge, SystemDesign, Coding, TellMeAboutYourself, Experience |
| Session modes | LiveInterview, MockInterview, Conversation |
| Overlay opacity default | 0.85 (slider range 0.30–1.00) |
| Latency target | 2–5 s to first answer token |
| Default STT | Whisper `ggml-small.en` |
| Default LLM | Ollama `llama3.2:3b` (flagged for review) |
| Ollama URL | `http://127.0.0.1:11434` |
| Audio format | PCM16, 16 kHz, mono per speaker |
| Coverage | min 75%, target 80% |
| Release channels | dev, beta (`vX.Y.Z-beta.N`), release (`vX.Y.Z`) |
| Export format | PDF |
