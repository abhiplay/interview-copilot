# ATLAS — Browser extension (`src/extension`)

## [A] Architect
- **Purpose:** companion overlay inside meeting tabs and start/stop control; all capture and AI happen in the desktop app (Modes A/B).
- **Constraints:** Manifest V3; Chrome, Edge, Firefox from one codebase; talks only to `http://127.0.0.1:5480`; requires pairing token from the desktop app.
- **Tech:** TypeScript, Vite build per target, `web-ext` for Firefox packaging.
- **Out of scope (v1):** tab audio capture (Mode C; Firefox cannot capture tab audio), store publishing.

## [T] Trace
1. User opens popup → enters pairing code shown in the desktop app → token stored in `storage.local`.
2. On a meeting page (Meet, Teams web, Zoom web) content script injects the overlay (Shadow DOM, opacity from settings).
3. Background worker opens `/ws/sessions/{id}/events` → forwards `answer.*` events to the content script.
4. Start/Stop buttons call backend REST.

## [L] Link
| From | To | Method | Contract | Failure mode |
|------|----|--------|----------|--------------|
| Background | Backend | WS + REST | `X-Pairing-Token` | backend down → overlay shows "Desktop app not running" |
| Content | Background | `runtime` messages | typed message union | tab closed → cleanup |

## [A] Assemble
1. Manifests + build. 2. Pairing popup. 3. Background WS client. 4. Content overlay. 5. Packaging in CI.

## [S] Stress-test
- Meeting page with strict CSP: overlay still renders (Shadow DOM, no inline scripts).
- Token revoked in desktop → extension prompts to re-pair.
