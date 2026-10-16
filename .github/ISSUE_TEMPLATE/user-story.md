---
name: User story (SDD → ATLAS → GOTCHA)
about: A unit of feature work. Every change starts as one of these.
title: "<MODULE>: <short actionable title>"
labels: ["type:feature"]
---

**Requirements:** FR-xx / NFR-xx
**Module:** <!-- Auth | Sessions | Documents | Audio | Transcription | Copilot | Practice | Transcripts | Providers | Overlay | Settings -->
**Spec file:** `specs/stories/<module>-<nn>-<slug>.md`

## Story

As a <user>, I want <capability>, so that <outcome>.

## Acceptance criteria

```gherkin
Scenario:
  Given
  When
  Then
```

## Out of scope

-

## ATLAS checklist

- [ ] Architect: boundaries + out of scope confirmed (`atlas/<component>.md`)
- [ ] Trace: data flow written
- [ ] Link: contracts + failure modes listed
- [ ] Assemble: build order agreed
- [ ] Stress-test: scenarios become tests

## GOTCHA prompt

```
=== GOALS ===
=== ORCHESTRATION ===
=== TOOLS ===
=== CONTEXT ===        see GOTCHA-PROJECT.md +
=== HEURISTICS ===     see GOTCHA-PROJECT.md +
=== ARGS ===
```

## Tasks

- [ ] Tests from acceptance criteria (written first)
- [ ] Implementation
- [ ] Coverage ≥ 75% for touched packages
- [ ] Docs updated
