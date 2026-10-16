# Architecture context — InterviewCopilot

Read with `CLAUDE.md` (layout, modules) and `docs/atlas-gotcha/interview-copilot.md` (trace, links).

## Backend layers (`src/backend`)

| Layer | Holds | May depend on |
|-------|-------|---------------|
| Domain | Entities (`Session`, `SessionDocument`, `TranscriptSegment`, `SuggestedAnswer`, `PracticeAttempt`, `UserAccount`), enums, domain exceptions | nothing |
| Application | Provider interfaces (`ISpeechToTextProvider`, `ILlmProvider`, `IAuthProvider`, `IPdfExporter`…), use cases, `AnswerPromptBuilder`, `QuestionTypeResolver` | Domain |
| Infrastructure | EF Core/SQLite, Whisper.net, Deepgram (disabled), Ollama, BYOK LLM clients, QuestPDF, password hashing | Application, Domain |
| Api | Minimal-API endpoints per module, WebSocket hubs, pairing-token middleware | all of the above |

`ArchTests` enforce these rules.

## Provider pattern

Each external capability has one Application interface and N Infrastructure adapters, picked by
configuration:

```
Transcription:Provider = Whisper | Deepgram      (Deepgram requires Transcription:Providers:Deepgram:Enabled=true)
Llm:Provider           = Ollama | Anthropic | OpenAI | Google
```

Cloud adapters receive the user's key **per request** from the desktop app (keychain); the backend
never persists keys.

## UI (`src/ui`)

`src/features/<module>/` per module (components, hooks, api). Platform access goes through
`src/platform/` (`desktop` bridge in Electron, `web` stub for Mode C). shadcn components in
`src/components/ui/`.

## Desktop (`src/desktop`)

Electron main process: backend process lifecycle, overlay window (transparent, always-on-top,
opacity setting), meeting detection + consent prompt, audio capture, keychain (`safeStorage`).
Preload exposes a narrow, typed `window.interviewCopilot` bridge — no `nodeIntegration`.

## Extension (`src/extension`)

MV3 companion: popup (pairing, start/stop), content script overlay in meeting tabs, background
service worker holding the WebSocket to the local backend. One codebase, two manifests (chromium,
firefox).
