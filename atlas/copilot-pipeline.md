# ATLAS — Copilot pipeline (Copilot, Practice, Providers)

## [A] Architect
- **Purpose:** detect questions, classify their type, generate grounded streaming answers; in practice mode, ask questions and score answers.
- **Constraints:** first token ≤ 5 s P95 with Ollama `llama3.2:3b` on reference hardware; prompt must fit the model context (budget tokens for docs); pinned type yields to auto-detect when confident.
- **Tech:** `ILlmProvider` (Ollama default; Anthropic/OpenAI/Google BYOK), `AnswerPromptBuilder`, `QuestionTypeResolver`, document chunk retrieval (keyword/BM25 in v1 — embeddings is an open question for the owner).
- **Out of scope:** fine-tuning, voice output, multi-language.

## [T] Trace — answer
1. Candidate-facing trigger: interviewer segment ends with a question (punctuation/intent) → `question.detected`.
2. Classify type (LLM short prompt or heuristic) → resolve against pinned type.
3. Retrieve top document chunks for the question.
4. Build prompt: system rules + session prompt + length + framework + type guidance + chunks + recent transcript.
5. Stream from `ILlmProvider` → `answer.delta` events → `answer.done` → persist `SuggestedAnswer`.
6. Regenerate/Shorter/Longer → same question, modifier applied → new answer version.

## [T] Trace — practice
1. Generate next question from JD + docs. 2. Candidate answers (mic). 3. Score 1–10 against the framework with feedback and an improved example. 4. Persist `PracticeAttempt`.

## [L] Link
| From | To | Method | Contract | Failure mode |
|------|----|--------|----------|--------------|
| Pipeline | Ollama | HTTP stream `127.0.0.1:11434/api/chat` | `ILlmProvider` | not running → `error` event "Start Ollama" |
| Pipeline | Cloud LLM | HTTPS | `ILlmProvider`, key per request | 401/429 → `error` event, no retries beyond 1 |

## [A] Assemble
1. Prompt builder + type resolver (pure, fully tested). 2. Ollama adapter. 3. Detection. 4. Streaming endpoint/events. 5. Answer modifiers. 6. Practice scoring. 7. BYOK adapters.

## [S] Stress-test
- Two questions in quick succession: second cancels first answer stream.
- 10 large documents: prompt stays within the model's context budget (truncation tested).
- Prompt injection inside an uploaded document ("ignore previous instructions"): treated as data, delimited in the prompt.
