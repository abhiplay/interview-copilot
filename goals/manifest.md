# Goals Manifest — InterviewCopilot

Goals describe **outcomes**, not implementation. Each goal maps to GitHub epics (one per module).

| ID | Goal | Measure of done | Status |
|----|------|-----------------|--------|
| G-01 | Live answer suggestions | First answer token ≤ 5 s (P95) after a question ends, on reference hardware, with local models | Planned |
| G-02 | Grounded, styled answers | Answers use attached docs (≤10), the session prompt, length and framework; question type auto-detected with pin override | Planned |
| G-03 | Zero-cost operation | Full flow works offline with Whisper + Ollama; no paid service required anywhere | Planned |
| G-04 | Practice mode | Mock interviewer asks JD-based questions, scores each answer and gives feedback | Planned |
| G-05 | History and export | Sessions/transcripts saved locally; any transcript exports to PDF | Planned |
| G-06 | Desktop + browser overlays | Semi-transparent always-on-top overlay (default 85%, slider) on Win/macOS; companion overlay in Chrome/Edge/Firefox | Planned |
| G-07 | Consent-first capture | Meeting detection prompts before capture; auto-start only when the user enables it | Planned |
| G-08 | Shareable open-source release | Unsigned beta/release installers published by CI; README lets a stranger install and run it | Planned |
| G-09 | Mode C readiness | Hosted mode can be added without moving UI code or changing Application interfaces | Ongoing constraint |

## Principles

1. Never build without a spec in `specs/` and an ATLAS checklist in `atlas/`.
2. One GOTCHA prompt per component/task, referencing `GOTCHA-PROJECT.md`.
3. Out-of-scope items (see `docs/atlas-gotcha/interview-copilot.md`) stay out until the owner says otherwise.
