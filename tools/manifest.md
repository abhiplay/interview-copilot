# Tools Manifest — InterviewCopilot

| Tool | Purpose | Usage |
|------|---------|-------|
| `tools/governance-report.sh` | Gartner success metrics: provenance completeness, bypass attempts, src↔test pairing. Exits non-zero on debt. | `./tools/governance-report.sh [path] [--json]` |
| `scripts/setup-github.sh` | Prints/creates the GitHub repo, labels and milestones (run it yourself) | `bash scripts/setup-github.sh` |
| `scripts/create-issues.sh` | Creates the initial epics and issues with `gh` (run it yourself) | `bash scripts/create-issues.sh` |

## Rules for adding tools

1. Check this manifest first — never write a script that already exists.
2. Name tools by action (`download-whisper-model.sh`, not `tool1.sh`).
3. Document here as soon as the tool is created.
4. Test before documenting.
5. One responsibility per tool.
