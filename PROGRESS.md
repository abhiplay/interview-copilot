# Scaffold report (2026-10-16)

Generation is complete. What to do next is in `NEXT-STEPS.md`. This file records the post-build
"What we adjusted" review; delete it once you've read it.

## Verification

| Check | Result |
|-------|--------|
| `dotnet format --verify-no-changes` | clean |
| Backend build (warnings as errors) + 115 tests | pass, **98.8% line / 95.1% branch** |
| `pnpm -r lint` / `pnpm -r typecheck` | clean |
| UI 36 tests / desktop 16 / extension 13 | pass, **99% / 93% / 100% lines** |
| UI production build, extension build (Chromium + Firefox) | built |
| Smoke run of the real backend | health, sign-in, sessions, providers, answer stream all OK (stream reports "Can't reach Ollama" until Ollama is installed) |
| Not run | Electron GUI launch, installers (release workflow), GitHub workflows (need the repo) |

## What we adjusted (against each GOTCHA layer)

- **Goals:** met for the scaffold. Live audio capture and transcription aren't wired yet. They're
  the first v0.2 issues, and questions are typed for now.
- **Orchestration:** generation followed scaffold.md's order, except autoskills (see Tools).
- **Tools:**
  - xunit v3 needs Microsoft Testing Platform on .NET 10, so coverage uses
    `Microsoft.Testing.Extensions.CodeCoverage` instead of coverlet.
  - FluentAssertions is pinned to 7.x (8.x needs a commercial licence).
  - MediatR was left out (its newer versions are commercially licensed). Plain services are used instead.
  - TypeScript is pinned to 6.0.3 because typescript-eslint doesn't support 7.x yet.
  - `autoskills` was **not run**: its latest release (0.4.1, published 2 days earlier) is broken
    (no package.json in the tarball). Retry later with `pnpm dlx autoskills -y`.
- **Context:**
  - SQLite instead of the default PostgreSQL (approved).
  - The template's `AnalysisMode=All` was relaxed to `latest-recommended`. All was too noisy to be useful.
  - The template's blanket `WebFetch` deny was dropped: in Claude Code, deny beats allow, so it would
    have blocked the allowlist too.
  - MCP lockdown only covers project servers (documented in GOVERNANCE.md).
  - `.claude/provenance/` is gitignored, as the template does it. This corrects what I said during
    the interview (that it would be committed).
- **Heuristics:** no paid provider is enabled by default, keys go only to the OS keychain, the
  backend binds to 127.0.0.1, `src/ui` has no Electron import (CI guard), capture asks for consent first.
- **Args:**
  - Port 5480, app ID `io.github.ukez15.interviewcopilot` and 85% opacity are used throughout.
  - Defaults I picked that are flagged for you: Ollama `llama3.2:3b`, the Firefox add-on ID, the Vite
    dev port 5173 (Vite's default), and the opacity slider range of 30–100%.
- **Bugs caught by tests during the build:**
  - EF Core treated domain-generated Guid keys as database-generated, so new documents were saved
    as UPDATEs. Fixed with `ValueGeneratedNever`.
  - Enums need to travel as names over JSON. Fixed with `JsonStringEnumConverter`.
  - Sandboxed Electron preloads can't `require` local files. Channel names are now inlined, with a
    test that keeps them in sync.
