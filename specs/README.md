# specs/ — Spec-Driven Development

SDD stages: **Specify → Plan → Execute**. Specs are the primary artifact; code follows them.

```
specs/
├── functional/requirements.md       ← FR-01..FR-18 captured at scaffold time
├── non-functional/requirements.md   ← NFR-01..NFR-14
├── templates/user-story.md          ← copy for every new story
└── stories/                         ← one file per story: <module>-<nn>-<slug>.md
```

Workflow per story:

1. **Specify:** copy `templates/user-story.md` into `stories/`, fill it in, and link the FR/NFR IDs.
2. **Plan:** fill the component checklist in `atlas/` and write the task GOTCHA (in the issue).
3. **Execute:** write the tests from the acceptance criteria first, then the code.
