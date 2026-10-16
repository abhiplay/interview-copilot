# GOTCHA-PROJECT.md — InterviewCopilot constitution

Permanent **Context** and **Heuristics** for every task in this repo. Task-level GOTCHA prompts
reference this file instead of repeating it. Change it only with the owner's explicit OK.

## Context

- Open source (MIT), public repo `ukez15/interview-copilot`. Contributors clone it and run it on
  their own machines.
- **Zero cost by default:** local Whisper + Ollama. Paid providers are opt-in BYOK and disabled by
  default. No feature may depend on a paid service, hosted server or signing certificate.
- v1 is **local only**: backend binds to `127.0.0.1:5480`, SQLite in the OS user-data folder.
- **Mode C (hosted, browser-only) is designed for but not built.** UI stays in `src/ui` with no
  direct Electron imports; auth, storage, STT and LLM stay behind Application interfaces.
- Platforms: Windows 10 22H2+/11, macOS 13+. Browsers (extension): Chrome, Edge, Firefox. Android
  is paused.
- Reference hardware: 16 GB RAM, no GPU.
- Users are candidates/sellers; their transcripts and documents are personal data.

## Heuristics

DO:
- Write the test first; keep coverage ≥75% (target 80%) in every package.
- Put every external service behind an `Application` interface with a fake in tests.
- Keep the backend on `127.0.0.1`; require the pairing token for extension requests.
- Store BYOK keys only via the desktop app's keychain bridge (`safeStorage`); pass them per request.
- Ask the user before starting capture (unless the auto-start setting is on).
- Stream answers (first token fast) instead of waiting for completion.
- Use constants/config for limits (10 docs, opacity range, timeouts).
- Use conventional commits and reference the issue (`Closes #N`).

DON'T:
- Don't enable Deepgram or any cloud LLM by default.
- Don't send audio, transcripts or documents off the machine unless a cloud provider is enabled.
- Don't log transcript text, document content or API keys.
- Don't import `electron` from `src/ui`.
- Don't add features that hide the app from screen sharing or proctoring tools.
- Don't use npm, npx or yarn. Don't use `^`/`~` versions.
- Don't push, skip hooks, or add AI attribution.
