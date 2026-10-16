# atlas/ — ATLAS checklists per component

One checklist per component (one GOTCHA per component, never one for the whole system). The
system-level ATLAS lives in `docs/atlas-gotcha/interview-copilot.md`.

| Component | Checklist | Modules |
|-----------|-----------|---------|
| Backend API | `backend.md` | Auth, Sessions, Documents, Transcripts, Settings |
| Transcription pipeline | `transcription-pipeline.md` | Transcription |
| Copilot pipeline | `copilot-pipeline.md` | Copilot, Practice, Providers |
| Desktop app | `desktop.md` | Audio, Overlay, Providers (keychain), Settings |
| Browser extension | `extension.md` | Overlay (companion), Copilot (display) |

Before a story's code is written, the relevant checklist section is reviewed and any new decisions
are added (asked of the owner, never assumed).
