# Non-functional requirements — InterviewCopilot v1

| ID | Category | Requirement | Verified by |
|----|----------|-------------|-------------|
| NFR-01 | Cost | Runs fully free and offline with local Whisper + Ollama. Paid providers are opt-in and off by default. | Integration test with fakes; manual offline run before release |
| NFR-02 | Privacy | No audio, transcript or document leaves the machine unless the user enables a cloud provider. | Code review checklist; network-egress test in CI for default config |
| NFR-03 | Security | BYOK keys stored only in the OS keychain (Windows Credential Manager / macOS Keychain) via Electron `safeStorage`; never in files, logs or the backend DB. | Unit tests on key store; log-redaction test |
| NFR-04 | Security | Local account passwords are hashed (PBKDF2 via ASP.NET Core `PasswordHasher`), never stored in plain text. | Unit tests |
| NFR-05 | Performance | End of question → first answer token: **2–5 s** (P95 ≤ 5 s) with default local models on reference hardware. | Latency benchmark test (manual on reference machine; CI with fakes for pipeline overhead ≤ 300 ms) |
| NFR-06 | Hardware | Reference: 16 GB RAM, any CPU from ~last 5 years, no GPU; 8 GB supported but slow. Default models: Whisper `small.en` + a 3–4B Ollama model. | Manual benchmark per release |
| NFR-07 | Platforms | Windows 10 22H2+/11, macOS 13+. Extension: Chrome, Edge, Firefox (companion only). | CI builds for win + mac; extension builds for both manifests |
| NFR-08 | Licence | MIT. | `LICENSE`, licence check in governance CI |
| NFR-09 | Usability | A stranger can install and run from the README alone (Ollama + model + app). | Release checklist |
| NFR-10 | Quality | ≥75% test coverage (target 80%) in backend and each TypeScript package, enforced in CI. | `ci.yml` coverage gates |
| NFR-11 | Network exposure | Backend binds to `127.0.0.1` only; extension requests require a pairing token; CORS limited to app/extension origins. | Integration tests |
| NFR-12 | Consent | No capture starts without a user action unless auto-start is explicitly enabled. | Unit tests on meeting-detection flow |
| NFR-13 | Releases | Unsigned installers; README documents SmartScreen/Gatekeeper steps. dev/beta/release channels. | `release.yml` |
| NFR-14 | Extensibility | Mode C must be addable without moving UI code or changing Application interfaces. | ArchTests + "no electron import in src/ui" lint rule |
