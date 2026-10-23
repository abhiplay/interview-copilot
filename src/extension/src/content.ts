import { createOverlay } from './content/overlay';
import { ANSWER_PORT, KEEPALIVE_INTERVAL_MS, type AnswerPortMessage, type StatusResponse } from './shared/messages';

// Injected into meeting pages (see manifest matches). Shows the companion overlay (Mode B, FR-18).
const overlay = createOverlay(document, {
  onAsk: (sessionId, question) => {
    const port = chrome.runtime.connect({ name: ANSWER_PORT });

    // Keep the background service worker alive while the answer streams. Chrome stops an idle MV3
    // worker after ~30s; a slow local model can stay silent for longer, and the worker dying takes
    // the in-flight request (and the whole answer) with it.
    const keepAlive = setInterval(() => {
      try {
        port.postMessage({ kind: 'ping' } satisfies AnswerPortMessage);
      } catch {
        clearInterval(keepAlive);
      }
    }, KEEPALIVE_INTERVAL_MS);

    port.onMessage.addListener((message: AnswerPortMessage) => {
      if (message.kind === 'event') {
        overlay.apply(message.event);
        if (message.event.type === 'done' || message.event.type === 'error') {
          clearInterval(keepAlive);
          port.disconnect();
        }
      }
    });
    port.postMessage({ kind: 'ask', sessionId, question } satisfies AnswerPortMessage);
  },
  onClose: () => overlay.host.remove(),
});

document.documentElement.append(overlay.host);
void chrome.runtime.sendMessage({ kind: 'status' }).then((status: StatusResponse) =>
  overlay.setSessions(status.sessions, status.signedIn ? status.error : 'Sign in from the InterviewCopilot extension popup.'),
);
