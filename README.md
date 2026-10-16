# InterviewCopilot

A free, open-source, real-time AI conversation assistant. It listens to live interviews, sales calls
or any conversation where freezing up isn't an option, and suggests answers within seconds. It also
has a **mock-interview practice mode** that asks you questions, then scores your answers and gives
feedback.

**Zero cost by default.** Everything runs on your own machine: speech-to-text with
[Whisper](https://github.com/ggerganov/whisper.cpp) and answers from a local model through
[Ollama](https://ollama.com). No account with any AI company is needed, and nothing leaves your
computer unless you turn on a cloud provider yourself.

> **Use it responsibly.** Some employers, assessment platforms and jurisdictions don't allow AI
> assistance or call transcription without everyone's consent. Check the rules for each conversation
> you use this in. InterviewCopilot always asks before it starts listening (unless you change that
> setting).

---

## Features

- **Live suggestions:** transcribes the conversation, detects each question, works out its type, and
  streams a suggested answer.
- **Question types, auto-detected:** behavioral, "how do you…", situational, technical knowledge,
  system design, coding, tell me about yourself, experience. Pin a type before a session; auto-detect
  takes over if the conversation shifts.
- **Answer style:** short, balanced or long, using STAR, SMART or no framework.
- **Your documents:** attach up to 10 per session (resume, job description, sales script, product
  docs…), and answers are grounded in them.
- **Session prompt:** tell the AI who you're talking to and how to answer.
- **Regenerate / Shorter / Longer** buttons on every answer.
- **Overlay:** a semi-transparent, always-on-top window (opacity slider, default 85%) on Windows and
  macOS, and optionally inside the meeting tab via the browser extension.
- **Practice mode:** the AI interviews you from the job description, then scores and critiques your
  answers.
- **History:** sessions and transcripts saved locally; export any transcript to PDF.

## How it works

```
 meeting audio ─┐
                ├─► Desktop app (Electron) ──► local backend (.NET, 127.0.0.1:5480)
 your mic ──────┘        │  overlay                ├─ Whisper (speech-to-text)
                         │                         ├─ question detection + type classification
 browser extension ◄─────┘  (optional overlay)     ├─ Ollama / your chosen model (answers)
                                                   └─ SQLite (sessions, transcripts, documents)
```

| Mode | What it is |
|------|------------|
| **A: Desktop** | The app notices a meeting starting, asks whether to start, and shows answers in a desktop overlay. |
| **B: Desktop + extension** | Same as A, plus answers appear inside the meeting tab in Chrome, Edge or Firefox. |
| **C: Browser-only** | *Planned.* A hosted version for people who don't want the desktop app (needs cloud models, so not free). |

## Requirements

| | Minimum |
|---|---|
| OS | Windows 10 (22H2) / 11, or macOS 13 Ventura or newer |
| RAM | 16 GB (8 GB works, slowly) |
| CPU/GPU | Any CPU from roughly the last 5 years. No GPU needed. Apple Silicon is fastest. |
| Disk | ~5 GB free for models |

Target speed: the first words of an answer appear **2–5 seconds** after a question ends, with the
default local models on the hardware above.

## Getting started

### Option 1: Install a release

Download the installer for your OS from
[Releases](https://github.com/ukez15/interview-copilot/releases) (`-beta` tags are pre-releases).

The installers are **not code-signed**, because signing costs money and this project stays free.
Your OS will warn you the first time:

- **Windows:** SmartScreen shows "Windows protected your PC". Click **More info → Run anyway**.
- **macOS:** right-click (or Control-click) the app → **Open** → **Open**. On macOS 15+, if that
  isn't offered, go to **System Settings → Privacy & Security** and click **Open Anyway**.

Then install [Ollama](https://ollama.com) and pull the default model:

```bash
ollama pull llama3.2:3b
```

The Whisper speech model is downloaded by the app on first run.

### Option 2: Run from source

Prerequisites: [Node.js 24+](https://nodejs.org), [pnpm 10+](https://pnpm.io),
[.NET 10 SDK](https://dotnet.microsoft.com), [Ollama](https://ollama.com).

```bash
git clone https://github.com/ukez15/interview-copilot.git
cd interview-copilot
pnpm install
ollama pull llama3.2:3b
pnpm dev:backend     # terminal 1: backend on http://127.0.0.1:5480
pnpm dev:desktop     # terminal 2: desktop app
```

For the full dev stack with the Aspire dashboard, run `pnpm dev` instead of `dev:backend`.

### Browser extension (optional, Mode B)

```bash
pnpm --filter @interview-copilot/extension build
```

- **Chrome / Edge:** open `chrome://extensions` (or `edge://extensions`), enable Developer mode,
  click **Load unpacked**, and select `src/extension/dist/chromium`.
- **Firefox:** open `about:debugging#/runtime/this-firefox`, click **Load Temporary Add-on**, and
  pick `src/extension/dist/firefox/manifest.json`.

The extension only works while the desktop app is running.

## Choosing providers

### Speech-to-text

| Provider | Cost | Status |
|---|---|---|
| **Whisper (local)** | Free | Default |
| **Deepgram** | Paid, with free signup credit | Built in, **disabled** by default |

Deepgram gives lower latency and better speaker separation. To turn it on:

1. Create a Deepgram account and API key.
2. In `src/backend/src/InterviewCopilot.Api/appsettings.json`, set
   `"Transcription": { "Providers": { "Deepgram": { "Enabled": true } } }`.
3. Restart the app, open **Settings → Providers**, choose **Deepgram**, and paste your key. It's
   stored in your OS keychain, never in a file.

### AI model

Ollama (local, free) is the default. You can switch models in **Settings → Providers**: any model
you've pulled into Ollama, or a cloud model (Anthropic, OpenAI, Google) with **your own API key**.
Cloud usage is billed to you by that provider.

## Privacy

- Audio, transcripts and documents stay on your machine by default.
- API keys are stored in Windows Credential Manager / macOS Keychain.
- Data lives in your user data folder (`%APPDATA%\InterviewCopilot` or
  `~/Library/Application Support/InterviewCopilot`). Delete it to remove everything.

## Contributing

Issues and PRs are welcome. Read [`CLAUDE.md`](CLAUDE.md) (project rules) and
[`GOVERNANCE.md`](GOVERNANCE.md) (quality gates) first. Use pnpm, conventional commits, and keep
test coverage at or above 75%.

## License

[MIT](LICENSE)
