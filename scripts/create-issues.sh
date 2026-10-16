#!/usr/bin/env bash
# Creates the initial epics (one per module) and their child issues for InterviewCopilot.
# Run it yourself after scripts/setup-github.sh:  bash scripts/create-issues.sh
# Each issue follows SDD → ATLAS → GOTCHA (see .github/ISSUE_TEMPLATE/user-story.md).
set -euo pipefail

REPO="ukez15/interview-copilot"

body() { # $1 = requirements, $2 = ATLAS checklist file, $3 = acceptance criteria (Gherkin)
  cat <<EOF
**Requirements:** $1
**Spec:** copy \`specs/templates/user-story.md\` into \`specs/stories/\` before starting.
**ATLAS checklist:** \`atlas/$2\`

## Acceptance criteria

\`\`\`gherkin
$3
\`\`\`

## Flow

- [ ] Specify: user story + acceptance criteria in \`specs/stories/\`
- [ ] ATLAS: review/extend \`atlas/$2\` (ask the owner about any new decision)
- [ ] GOTCHA prompt written (references \`GOTCHA-PROJECT.md\`)
- [ ] Tests first, then code; coverage ≥ 75%
EOF
}

epic() { # $1 module, $2 title, $3 layer, $4 milestone → prints issue number
  gh issue create --repo "$REPO" --title "Epic: $2" --milestone "$4" \
    --label "type:epic,module:$1,layer:$3,priority:high" \
    --body "Epic for the **$2** module. Child issues reference this one." | grep -oE '[0-9]+$'
}

task() { # $1 epic#, $2 module, $3 title, $4 labels, $5 milestone, $6 reqs, $7 atlas, $8 criteria
  gh issue create --repo "$REPO" --title "$3" --milestone "$5" --label "module:$2,$4" \
    --body "Part of #$1

$(body "$6" "$7" "$8")" >/dev/null
  echo "  created: $3"
}

echo "Auth"
E=$(epic auth "Auth" auth "v0.3 Supporting")
task "$E" auth "Auth: persist sign-in across app restarts (secure token storage)" "type:feature,layer:desktop,priority:medium" "v0.3 Supporting" "FR-12, NFR-03" desktop.md \
  "Given I signed in before, When I reopen the app, Then I'm still signed in without my password being stored"

echo "Sessions"
E=$(epic sessions "Sessions" backend "v0.2 Core")
task "$E" sessions "Sessions: edit settings of an existing session from the UI" "type:feature,layer:frontend,priority:medium" "v0.2 Core" "FR-03..FR-06" backend.md \
  "Given a draft session, When I change length, framework, pinned type or prompt, Then the next answer uses the new settings"

echo "Documents"
E=$(epic documents "Documents" backend "v0.2 Core")
task "$E" documents "Documents: extract text from PDF and DOCX (choose library with owner)" "type:feature,layer:backend,priority:high" "v0.2 Core" "FR-02" backend.md \
  "Given a resume.pdf or jd.docx, When I attach it, Then its text is used to ground answers"

echo "Audio"
E=$(epic audio "Audio" desktop "v0.2 Core")
task "$E" audio "Audio: capture system audio (interviewer) and mic (candidate) on Windows" "type:feature,layer:desktop,priority:critical" "v0.2 Core" "FR-07" desktop.md \
  "Given a live session on Windows, When the other side speaks, Then PCM16 16 kHz frames tagged Interviewer reach the backend"
task "$E" audio "Audio: capture system audio and mic on macOS 13+ (Screen Recording permission)" "type:feature,layer:desktop,priority:critical" "v0.2 Core" "FR-07" desktop.md \
  "Given macOS without the permission, When I start a session, Then I'm told exactly how to grant it and mic-only still works"
task "$E" audio "Audio: start a session from the meeting-detected prompt" "type:feature,layer:desktop,priority:high" "v0.4 Integration" "FR-16, NFR-12" desktop.md \
  "Given Zoom starts, When I click Start in the prompt, Then I can pick or create a session and capture begins; nothing starts without my click unless auto-start is on"

echo "Transcription"
E=$(epic transcription "Transcription" backend "v0.2 Core")
task "$E" transcription "Transcription: audio WebSocket + voice-activity buffering" "type:feature,layer:backend,priority:critical" "v0.2 Core" "FR-07" transcription-pipeline.md \
  "Given streamed frames, When a speaker pauses ≥600 ms, Then one transcript segment is persisted and published"
task "$E" transcription "Transcription: Whisper model download with progress in Settings" "type:feature,layer:frontend,priority:high" "v0.2 Core" "NFR-01, NFR-09" transcription-pipeline.md \
  "Given no model on disk, When I open the app, Then I see download progress for ggml-small.en"
task "$E" transcription "Transcription: Deepgram streaming (WebSocket) behind the existing flag" "type:feature,layer:backend,priority:low" "v0.4 Integration" "FR-15" transcription-pipeline.md \
  "Given Deepgram enabled in config and a key in Settings, When I select it, Then segments come from Deepgram"

echo "Copilot"
E=$(epic copilot "Copilot" backend "v0.2 Core")
task "$E" copilot "Copilot: auto-answer detected interviewer questions from the live transcript" "type:feature,layer:backend,priority:critical" "v0.2 Core" "FR-08, FR-09" copilot-pipeline.md \
  "Given a live session, When the interviewer asks a question, Then an answer starts streaming to the overlay within 5 s (P95)"
task "$E" copilot "Copilot: events WebSocket for overlay and extension" "type:feature,layer:backend,priority:high" "v0.2 Core" "FR-17, FR-18" copilot-pipeline.md \
  "Given two clients on a session, When an answer streams, Then both receive answer.delta events"
task "$E" copilot "Copilot: latency benchmark against NFR-05" "type:task,layer:backend,priority:medium" "v0.4 Integration" "NFR-05" copilot-pipeline.md \
  "Given reference hardware and default models, When 20 questions are asked, Then P95 first-token latency ≤ 5 s is recorded in docs"

echo "Practice"
E=$(epic practice "Practice" backend "v0.3 Supporting")
task "$E" practice "Practice: mock interviewer asks, listens, scores and gives feedback" "type:feature,layer:backend,priority:high" "v0.3 Supporting" "FR-11" copilot-pipeline.md \
  "Given a mock session with a JD, When I answer a question aloud, Then I get a 1-10 score and feedback that is saved"

echo "Transcripts"
E=$(epic transcripts "Transcripts" frontend "v0.3 Supporting")
task "$E" transcripts "Transcripts: history view of transcript + answers" "type:feature,layer:frontend,priority:medium" "v0.3 Supporting" "FR-13" backend.md \
  "Given an ended session, When I open it, Then I see the transcript and suggested answers in order"

echo "Providers"
E=$(epic providers "Providers" backend "v0.3 Supporting")
task "$E" providers "Providers: BYOK adapters for Anthropic, OpenAI and Google" "type:feature,layer:backend,priority:medium" "v0.3 Supporting" "FR-15, NFR-03" copilot-pipeline.md \
  "Given a key saved in the keychain, When I choose a cloud model, Then answers stream from it and the key is never logged or stored by the backend"
task "$E" providers "Providers: choose any pulled Ollama model in Settings" "type:feature,layer:frontend,priority:medium" "v0.3 Supporting" "FR-15" copilot-pipeline.md \
  "Given models pulled in Ollama, When I open Settings, Then I can pick one and it's used for answers"

echo "Overlay"
E=$(epic overlay "Overlay" desktop "v0.4 Integration")
task "$E" overlay "Overlay: extension pairing with the desktop app" "type:feature,layer:extension,priority:medium" "v0.4 Integration" "FR-18, NFR-11" extension.md \
  "Given the desktop app shows a pairing code, When I enter it in the extension, Then the extension gets a scoped token without my password"

echo "Settings"
E=$(epic settings "Settings" frontend "v0.3 Supporting")
task "$E" settings "Settings: show where local data lives and how to delete it" "type:feature,layer:frontend,priority:low" "v0.3 Supporting" "NFR-02" desktop.md \
  "Given Settings, When I open Privacy, Then I see the data folder path and a button that opens it"

echo "Release"
gh issue create --repo "$REPO" --title "Release: first beta (v0.1.0-beta.1) with install docs checked by a fresh user" \
  --milestone "v1.0 Release" --label "type:task,layer:ci-cd,priority:high" \
  --body "$(body "NFR-09, NFR-13" desktop.md "Given a clean Windows and macOS machine, When someone follows the README, Then they get answers with only free software installed")" >/dev/null
echo "  created: Release: first beta"

echo "Done."
