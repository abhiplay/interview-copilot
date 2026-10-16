# ATLAS — Desktop app (`src/desktop` + `src/ui`)

## [A] Architect
- **Purpose:** the product users run: session setup, overlay, history, settings; captures audio; starts the backend; detects meetings.
- **Constraints:** Windows 10 22H2+/11, macOS 13+; overlay transparent + always on top, opacity 0.30–1.00, default 0.85; `contextIsolation` on, `nodeIntegration` off; no Electron import in `src/ui`.
- **Tech:** Electron, electron-builder (NSIS + DMG, unsigned), React UI from `src/ui`, `safeStorage` for keys, `desktopCapturer` loopback audio (Windows) / ScreenCaptureKit-backed loopback (macOS 13+ — verify support in the pinned Electron version before building).
- **Out of scope:** auto-update, code signing, Linux builds (not requested).

## [T] Trace — meeting detected
1. Poll running processes every 5 s for known meeting apps (Zoom, Teams, Webex, Slack huddles; browser meeting tabs via extension).
2. Match → if `autoStart` off: system notification + in-app prompt. If on: start directly.
3. User confirms → start backend session → open overlay → begin capture.

## [L] Link
| From | To | Method | Contract | Failure mode |
|------|----|--------|----------|--------------|
| Renderer | Main | IPC via preload | typed `window.interviewCopilot` | — |
| Main | Backend process | child process | spawn `InterviewCopilot.Api` with `--urls http://127.0.0.1:5480` | crash → restart ×3, then error UI |
| Main | Keychain | `safeStorage` | encrypt/decrypt API keys | unavailable → refuse to store |

## [A] Assemble
1. Main window + preload bridge. 2. Backend lifecycle. 3. Overlay window + opacity. 4. Audio capture. 5. Meeting detection + consent prompt. 6. Keychain. 7. Packaging.

## [S] Stress-test
- macOS without Screen Recording permission → explain how to grant, mic-only fallback.
- Overlay over a full-screen meeting app on both OSes.
- Backend port 5480 already in use → clear error naming the port.
