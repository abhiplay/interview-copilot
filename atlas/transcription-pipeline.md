# ATLAS — Transcription pipeline (Transcription module)

## [A] Architect
- **Purpose:** turn two labelled audio streams (interviewer = system audio, candidate = mic) into ordered transcript segments in near real time.
- **Constraints:** default Whisper (`ggml-small.en`) on CPU; segment latency ≤ 1.5 s after speech pause; Deepgram available only when `Transcription:Providers:Deepgram:Enabled=true`.
- **Tech:** Whisper.net (whisper.cpp bindings), Deepgram streaming API (disabled), `ISpeechToTextProvider`.
- **Out of scope:** diarization within one stream (speaker comes from the stream source), languages other than English in v1 (open question for the owner).

## [T] Trace
1. Desktop sends PCM16/16 kHz frames tagged with a speaker byte over `/ws/sessions/{id}/audio`.
2. Per-speaker voice-activity detection buffers until a pause (≥ 600 ms) or 15 s max.
3. Buffer → `ISpeechToTextProvider.TranscribeAsync` → text.
4. Segment persisted (session, speaker, start/end ms, text) and published as `transcript.segment`.

## [L] Link
| From | To | Method | Contract | Failure mode |
|------|----|--------|----------|--------------|
| Desktop main | Api WS | binary frames | `[speaker:1 byte][pcm16…]` | disconnect → client buffers ≤ 5 s |
| Pipeline | Whisper.net | in-process | `ISpeechToTextProvider` | model missing → `error` event with download action |
| Pipeline | Deepgram | WSS | `ISpeechToTextProvider` | disabled → provider not registered |

## [A] Assemble
1. Provider interface + fake. 2. VAD/buffering (pure, unit-tested). 3. Whisper adapter. 4. WS endpoint. 5. Deepgram adapter behind flag.

## [S] Stress-test
- 60-minute session: no unbounded buffers (memory flat).
- Silence for 10 minutes: no empty segments emitted.
- Both speakers talking over each other: two independent segments, ordered by start time.
