# Functional requirements — InterviewCopilot v1

Captured during the scaffold interview (2026-10-15).

## Session setup

| ID | Requirement | Module |
|----|-------------|--------|
| FR-01 | The user creates a session and chooses a mode: **Live interview**, **Mock interview practice**, or **Sales/general conversation**. | Sessions |
| FR-02 | The user attaches **up to 10 documents** per session (resume, JD, sales script, product docs, any other) for the AI to ground answers in. An 11th document is rejected with a clear message. | Documents |
| FR-03 | The user picks the **answer length**: short, balanced or long. | Sessions |
| FR-04 | The user picks the **answer framework**: STAR, SMART, or none. | Sessions |
| FR-05 | The user can **pin a question type** before starting: behavioral, "how do you…", situational, technical knowledge, system design, coding, tell me about yourself, experience. | Sessions |
| FR-06 | The user writes a **session prompt** (who they're talking to, tone, how to answer). It applies to every answer and is used as context for question-type detection. | Sessions |

## Live assistance

| ID | Requirement | Module |
|----|-------------|--------|
| FR-07 | The app transcribes the conversation in real time and labels the speaker (interviewer vs. user). | Audio, Transcription |
| FR-08 | The AI detects when a question is asked and auto-classifies its type. **Auto-detection overrides the pinned type** when the question shifts. | Copilot |
| FR-09 | The AI streams a suggested answer using the documents, length, framework and session prompt. | Copilot |
| FR-10 | Each answer has **Regenerate**, **Shorter** and **Longer** buttons in the app. | Copilot, Overlay |
| FR-16 | When a meeting is detected, the desktop app **prompts** "Meeting detected — start InterviewCopilot?". Fully automatic start is an opt-in setting. | Audio, Settings |
| FR-17 | Answers show in a **semi-transparent always-on-top overlay** on Windows and macOS with an **opacity slider** (default 85%). | Overlay |
| FR-18 | With the browser extension installed and paired, answers also show as an overlay **inside the meeting tab** (Chrome, Edge, Firefox), with start/stop controls. | Overlay |

## Practice mode

| ID | Requirement | Module |
|----|-------------|--------|
| FR-11 | In mock interview mode, the AI acts as interviewer, asks questions based on the JD and documents, then **scores** each spoken answer and gives **feedback**. | Practice |

## Data

| ID | Requirement | Module |
|----|-------------|--------|
| FR-12 | Local user accounts (username + password). | Auth |
| FR-13 | Sessions and transcripts are saved locally and can be browsed later. | Transcripts |
| FR-14 | Export a transcript, including the suggested answers, to **PDF**. | Transcripts |

## Settings

| ID | Requirement | Module |
|----|-------------|--------|
| FR-15 | Choose the STT provider (Whisper default; Deepgram selectable only when enabled in config) and the LLM provider/model (Ollama default; BYOK Anthropic/OpenAI/Google). | Providers, Settings |

## Future (Mode C — not in v1)

- Hosted backend + web frontend; browser-only use with cloud models.
- Google/GitHub sign-in.
- Cloud storage of transcripts and share links.
